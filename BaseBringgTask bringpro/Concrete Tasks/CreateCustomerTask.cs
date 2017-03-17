using Microsoft.Practices.Unity;
using Newtonsoft.Json.Linq;
using Sabio.Web.Classes.Tasks.Bringg.Interfaces;
using Sabio.Web.Models.Requests.Bringg;
using Sabio.Web.Services;
using Sabio.Web.Services.Interfaces;
using System.Collections.Generic;

namespace Sabio.Web.Classes.Tasks.Bringg
{
    public class CreateCustomerTask<T> : BaseBringgTask<T>,IBringgTask<T>
        where T : RegisterBringgRequest//, IBringgTask<T>
    {
        [Dependency]
        public IBringgUserService _BringgUserService { get; set; }


        public override string GetRequestUrl(T request)
        {
            return "customers";
        }

        public override string GetRequestType()
        {
            return "POST";
        }

        protected override Dictionary<string, object> MakeRequest(T request)
        {          
            // Create the payload object
            Dictionary<string, object> payload = new Dictionary<string, object>();

            //Set the data that will be sent
            payload.Add("name", request.Name);
            payload.Add("address", request.Address);
            payload.Add("phone", request.Phone);
            payload.Add("email", request.Email);
                        
            return payload;

        }

        protected override void ProcessResponse(T request, Dictionary<string, object> response)
        {

            string userId = request.UserId;

            IDictionary<string, JToken> customer = null;
            int externalId = 0;

            if (response.ContainsKey("customer"))
            {
                customer = (JObject)response["customer"];
                if (customer.ContainsKey("id"))
                {
                    externalId = (int)customer["id"];
                }
            }

            string stringId = externalId.ToString();

            _BringgUserService.UpdateExternalId(userId, stringId);

        }
    }
}