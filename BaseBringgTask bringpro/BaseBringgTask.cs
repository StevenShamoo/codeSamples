using Sabio.Web.Classes.Extensions;
using Sabio.Web.Models.Requests.Bringg;
using Sabio.Web.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Sabio.Web.Classes.Tasks.Bringg
{
    public abstract class BaseBringgTask<T> where T : BaseBringgRequest 
    {

        public abstract string GetRequestType();
        public abstract string GetRequestUrl(T request);

        protected abstract Dictionary<string, object> MakeRequest(T request);
        //Process Request will look like the function above, it will have a 2nd input, which will be what the dictionary thing that Execute() is now returning
        protected abstract void ProcessResponse(T request, Dictionary<string, object> response);


        public Dictionary<string, object> Execute(T request)
        {
            Dictionary<string, object> payload = MakeRequest(request);


            payload.Add("company_id", ConfigService.CompanyId);

            // set the system data that will be sent.
            payload.Add("timestamp", DateTime.UtcNow.ToUnixTimeSeconds().ToString());
            payload.Add("access_token", ConfigService.BringgAccessToken);


            try
            {
                // make the parameters into a url encoded query string
                StringBuilder queryParams = new StringBuilder();

                foreach (string key in payload.Keys)
                    queryParams.Append(string.Concat(queryParams.Length > 0 ? "&" : "", key, "=", payload[key].ToEncodedUrlString()));


                // ## THE BELOW CODE WILL ENCRYPT THE ENTIRE FILE AND VERIFY THE HASH ##
                // sign the query string using an HmacSHA1 mechanism with your secret key and add the signature to your parameters
                using (MemoryStream inStream = new MemoryStream(queryParams.ToByteArray()))
                using (MemoryStream outStream = new MemoryStream())
                {
                    // Use the secret key to sign the message file.
                    SignStream(ConfigService.BringgSecretKey.ToByteArray(), inStream, outStream);

                    // Verify the signed file
                    if (VerifyStream(ConfigService.BringgSecretKey.ToByteArray(), outStream))
                    {
                        // Add the new signature into the original payload.
                        payload.Add("signature", outStream.ToHashString(20));
                    }
                }

                // Create HTTP POST request to an endpoint:
                //string endpoint = "customers";//"customers";

                // get the url;
                string url = string.Concat(ConfigService.BaseUrl, GetRequestUrl(request));
                // create the request
                HttpWebRequest requestForm = WebRequest.Create(url) as HttpWebRequest;
                // add the content type
                requestForm.ContentType = "application/json";
                // add the method we will be using
                requestForm.Method = GetRequestType();
                // serialize it as a json object
                string jsonPackage = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                // set the length
                requestForm.ContentLength = jsonPackage.Length;


                // write the payload to the request stream
                using (StreamWriter writer = new StreamWriter(requestForm.GetRequestStream()))
                {
                    // write the package to the stream
                    writer.Write(jsonPackage);

                    // flush & close the stream
                    writer.Flush();
                    writer.Close();
                }

                // process the response
                using (HttpWebResponse response = requestForm.GetResponse() as HttpWebResponse)
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    // get the full response string
                    string responseString = reader.ReadToEnd();

                    // attempt to convert it to the response model
                    Dictionary<string, object> responseModel = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);

                    ProcessResponse(request, responseModel);
                    // Do whatever we want with the response modal.
                    return responseModel;


                }


            }
            catch (Exception e)
            {
                throw e;

            }


        }

        #region Signing & encrypting the stream

        // Computes a keyed hash for a source file and saves to target stream with the keyed hash
        // prepended to the contents of the source file. 
        private static void SignStream(byte[] key, Stream inStream, Stream outStream)
        {
            // Initialize the keyed hash object.
            using (HMACSHA1 hmac = new HMACSHA1(key))
            {
                // Compute the hash of the input file.
                byte[] hashValue = hmac.ComputeHash(inStream);

                // Reset inStream to the beginning of the file.
                inStream.Position = 0;

                // Write the computed hash value to the output file.
                outStream.Write(hashValue, 0, hashValue.Length);

                // Copy the contents of the sourceFile to the destFile.
                int bytesRead;

                // read 1K at a time
                byte[] buffer = new byte[1024];

                do
                {
                    // Read from the wrapping CryptoStream.
                    bytesRead = inStream.Read(buffer, 0, 1024);
                    outStream.Write(buffer, 0, bytesRead);
                } while (bytesRead > 0);
            }
        }

        // Compares the key in the source file with a new key created for the data portion of the file. If the keys 
        // compare the data has not been tampered with.
        public static bool VerifyStream(byte[] key, Stream inStream)
        {
            // Initialize the keyed hash object. 
            using (HMACSHA1 hmac = new HMACSHA1(key))
            {
                // Create an array to hold the keyed hash value read from the file.
                byte[] storedHash = new byte[hmac.HashSize / 8];

                // Reset inStream to the beginning of the file.
                inStream.Position = 0;

                // Read in the storedHash.
                inStream.Read(storedHash, 0, storedHash.Length);

                // Compute the hash of the remaining contents of the file.
                // The stream is properly positioned at the beginning of the content, 
                // immediately after the stored hash value.
                byte[] computedHash = hmac.ComputeHash(inStream);

                // compare the computed hash with the stored value
                for (int i = 0; i < storedHash.Length; i++)
                {
                    if (computedHash[i] == storedHash[i])
                        continue;

                    //  Console.WriteLine("Hash values differ! Signed file has been tampered with!");
                    return false;
                }
            }

            //Console.WriteLine("Hash values agree -- no tampering occurred.");
            return true;
        } //end VerifyFile

        #endregion



    }
}