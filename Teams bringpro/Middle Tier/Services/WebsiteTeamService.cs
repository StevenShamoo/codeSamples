using Microsoft.Practices.Unity;
using Sabio.Data;
using Sabio.Web.Classes.Tasks.Bringg;
using Sabio.Web.Classes.Tasks.Bringg.Interfaces;
using Sabio.Web.Domain;
using Sabio.Web.Models.Requests;
using Sabio.Web.Models.Requests.Bringg;
using Sabio.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Sabio.Web.Services
{
    public class WebsiteTeamService : BaseService, IWebsiteTeamService
    {

        [Dependency("CreateTeamTask")]
        public IBringgTask<WebsiteTeamInsertRequest> _CreateTeamTask { get; set; }

        [Dependency("UpdateTeamTask")]
        public IBringgTask<BringgTeamRequest> _UpdateTeamTask { get; set; }

        [Dependency("DeleteTeamTask")]
        public IBringgTask<BringgTeamRequest> _DeleteTeamTask { get; set; }


        public void WebsiteTeamInsert(WebsiteTeamInsertRequest model)
        {

            int id = 0;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.WebsiteTeams_Insert"
            , inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Name", model.Name);
                paramCollection.AddWithValue("@Description", model.Description);
                paramCollection.AddWithValue("@WebsiteId", model.WebsiteId);
                paramCollection.AddWithValue("@ParentTeamId", model.Parent_Team_Id);
                paramCollection.AddWithValue("@AddressId", model.AddressId);


                SqlParameter s = new SqlParameter("@WebsiteIds", SqlDbType.Structured);
                if (model.WebsiteIds != null && model.WebsiteIds.Any())
                {
                    s.Value = new IntIdTable(model.WebsiteIds);
                }
                paramCollection.Add(s);

                SqlParameter z = new SqlParameter("@ZipCodes", SqlDbType.Structured);
                if (model.ZipCodes != null && model.ZipCodes.Any())
                {
                    z.Value = new NVarcharTable(model.ZipCodes);
                }
                paramCollection.Add(z);


                SqlParameter p = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                p.Direction = System.Data.ParameterDirection.Output;

                paramCollection.Add(p);

            }, returnParameters: delegate (SqlParameterCollection param)
            {
                int.TryParse(param["@Id"].Value.ToString(), out id);
            });


            WebsiteTeamInsertRequest bringgTeamCreateRequest = new WebsiteTeamInsertRequest();
            bringgTeamCreateRequest.Name = model.Name;
            bringgTeamCreateRequest.Description = model.Description;
            bringgTeamCreateRequest.WebsiteTeamId = id;
            _CreateTeamTask.Execute(bringgTeamCreateRequest);
        }

        public List<WebsiteTeam> WebsiteTeamGet(int websiteId)
        {

            List<WebsiteTeam> WebsiteTeams = new List<WebsiteTeam>();
            DataProvider.ExecuteCmd(GetConnection, "dbo.WebsiteTeams_SelectAll"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@WebsiteId", websiteId);

                }
                , map: delegate (IDataReader reader, short set)
                {
                    WebsiteTeam w = new WebsiteTeam();
                    int startingIndex = 0;
                    w.Id = reader.GetSafeInt32(startingIndex++);
                    w.Name = reader.GetSafeString(startingIndex++);
                    w.WebsiteId = reader.GetSafeInt32(startingIndex++);
                    w.Description = reader.GetSafeString(startingIndex++);
                    w.Parent_Team_Id = reader.GetSafeInt32(startingIndex++);
                    w.ExternalTeamId = reader.GetSafeInt32(startingIndex++);
                    w.AddressId = reader.GetSafeString(startingIndex++);


                    WebsiteTeams.Add(w); 
                }
                );
            return WebsiteTeams;
        }
        public WebsiteTeam WebsiteTeamGetById(int id)
        {

            WebsiteTeam w = null;
            List<int> Websites = new List<int>();
            List<string> ZipCodes = new List<string>();
            DataProvider.ExecuteCmd(GetConnection, "dbo.WebsiteTeams_SelectById"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@Id", id);

                }
                , map: delegate (IDataReader reader, short set)
                {
                    if (set == 0) {
                        w = new WebsiteTeam();
                        int startingIndex = 0;
                        w.Id = reader.GetSafeInt32(startingIndex++);
                        w.Name = reader.GetSafeString(startingIndex++);
                        w.WebsiteId = reader.GetSafeInt32(startingIndex++);
                        w.Description = reader.GetSafeString(startingIndex++);
                        w.Parent_Team_Id = reader.GetSafeInt32(startingIndex++);
                        w.ExternalTeamId = reader.GetSafeInt32(startingIndex++);
                        w.AddressId = reader.GetSafeString(startingIndex++);
                    }
                    else if (set == 1)
                    {
                        int startingIndex = 0;
                        Websites.Add(reader.GetSafeInt32(startingIndex++));
                    }

                    else if (set == 2)
                    {
                        int startingIndex = 0;
                        ZipCodes.Add(reader.GetSafeString(startingIndex++));
                    }



                }
                );
            w.WebsiteIds = Websites.ToArray();
            w.ZipCodes = ZipCodes.ToArray();
            return w;
        }

        public void WebsiteTeamUpdate(int id, WebsiteTeamInsertRequest model)
        {


            DataProvider.ExecuteNonQuery(GetConnection, "dbo.WebsiteTeams_Update"
            , inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
                paramCollection.AddWithValue("@Name", model.Name);
                paramCollection.AddWithValue("@Description", model.Description);
                paramCollection.AddWithValue("@WebsiteId", model.WebsiteId);
                paramCollection.AddWithValue("@ParentTeamId", model.Parent_Team_Id);
                paramCollection.AddWithValue("@AddressId", model.AddressId);
                paramCollection.AddWithValue("@OldAddressId", model.OldAddressId);


                SqlParameter s = new SqlParameter("@WebsiteIds", SqlDbType.Structured);
                if (model.WebsiteIds != null && model.WebsiteIds.Any())
                {
                    s.Value = new IntIdTable(model.WebsiteIds);
                }
                paramCollection.Add(s);

                SqlParameter z = new SqlParameter("@ZipCodes", SqlDbType.Structured);
                if (model.ZipCodes != null && model.ZipCodes.Any())
                {
                    z.Value = new NVarcharTable(model.ZipCodes);
                }
                paramCollection.Add(z);

            }
            );
            BringgTeamRequest TeamUpdate = new BringgTeamRequest();
            TeamUpdate.Name = model.Name;
            TeamUpdate.Description = model.Description;
            TeamUpdate.Parent_Team_Id = model.ExternalTeamId;
            _UpdateTeamTask.Execute(TeamUpdate);
            
        }

        public void WebsiteTeamDelete(int id, int externalTeamId)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.WebsiteTeams_Delete"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@Id", id);

                }
                );
            BringgTeamRequest TeamDelete = new BringgTeamRequest();
            TeamDelete.Parent_Team_Id = externalTeamId;
            _DeleteTeamTask.Execute(TeamDelete);
        }

        public void UpdateExternalTeamId(int Id, int ExternalTeamId)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.WebsiteTeams_InsertExternal",
            inputParamMapper: delegate (SqlParameterCollection paramColection)
            {
                paramColection.AddWithValue("@Id", Id);
                paramColection.AddWithValue("@ExternalTeamId", ExternalTeamId);
            });


        }

        public List<WebsiteTeam> GetAllTeams()
        {
            List<WebsiteTeam> TeamList = new List<WebsiteTeam>();

            DataProvider.ExecuteCmd(GetConnection, "dbo.WebsiteTeams_SelectAllTeams"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {

              }, map: delegate (IDataReader reader, short set)
              {

                  WebsiteTeam w = new WebsiteTeam();
                  int startingIndex = 0;
                  w.Id = reader.GetSafeInt32(startingIndex++);
                  w.Name = reader.GetSafeString(startingIndex++);
                  w.WebsiteId = reader.GetSafeInt32(startingIndex++);
                  w.Description = reader.GetSafeString(startingIndex++);
                  w.Parent_Team_Id = reader.GetSafeInt32(startingIndex++);
                  w.ExternalTeamId = reader.GetSafeInt32(startingIndex++);

                  TeamList.Add(w);

              });


            return TeamList;
        }


        public int GetTeamIdByZipcode(string ZipCode)
        {
            int c = 0;

            DataProvider.ExecuteCmd(GetConnection, "dbo.WebsiteTeamsZipCodes_SelectByZipCode"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {
                  paramCollection.AddWithValue("@ZipCode", ZipCode);

              }, map: delegate (IDataReader reader, short set)
              {
                  c = reader.GetSafeInt32(0);
              }

              );

            return c;
        }


    }
}