using Microsoft.Practices.Unity;
using Sabio.Web.Domain;
using Sabio.Web.Models.Requests;
using Sabio.Web.Models.Responses;
using Sabio.Web.Services;
using Sabio.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sabio.Web.Controllers.Api
{
    [RoutePrefix("api/websiteteam")]
    public class WebsiteTeamApiController : ApiController
    {

        [Dependency]
        public IWebsiteTeamService _WebsiteTeamService { get; set; }

        [Route("insert") HttpPost]
        public HttpResponseMessage AddWebsiteTeam(WebsiteTeamInsertRequest model)
        {

            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }
            //ItemResponse<bool> response = new ItemResponse<bool>();
            //response.Item = _WebsiteTeamService.WebsiteTeamInsert(model);

            _WebsiteTeamService.WebsiteTeamInsert(model);
            return Request.CreateResponse(HttpStatusCode.OK, "Team Successfully Added");
        }

        [Route("get/{websiteId:int}"), HttpGet]
        public HttpResponseMessage GetWebsiteTeams(int websiteId)
        {
            ItemsResponse <WebsiteTeam> response = new ItemsResponse<WebsiteTeam>();

            response.Items = _WebsiteTeamService.WebsiteTeamGet(websiteId);

            return Request.CreateResponse(response);

        }
        [Route("getbyid/{id:int}"), HttpGet]
        public HttpResponseMessage GetWebsiteTeamById(int id)
        {
            ItemResponse<WebsiteTeam> response = new ItemResponse<WebsiteTeam>();

            response.Item = _WebsiteTeamService.WebsiteTeamGetById(id);

            return Request.CreateResponse(response);

        }

        [Route("update/{Id:int}"), HttpPut]
        public HttpResponseMessage UpdateWebsiteTeam(int Id, WebsiteTeamInsertRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }

            _WebsiteTeamService.WebsiteTeamUpdate(Id, model);
            //ItemResponse<int> response = new ItemResponse<int>();
            //response.Item = SettingsServices.Update(Id, model);
            return Request.CreateResponse(HttpStatusCode.OK, "Team Updated Successfully");
        }

        [Route("delete/{Id:int}"), HttpDelete]
        public HttpResponseMessage DeleteWebsiteTeam(int Id)
        {

            WebsiteTeam Team = new WebsiteTeam();

            Team = _WebsiteTeamService.WebsiteTeamGetById(Id);
            _WebsiteTeamService.WebsiteTeamDelete(Id, Team.ExternalTeamId);

            return Request.CreateResponse(HttpStatusCode.OK, "Team Deleted Successfully");
        }

        [Route(), HttpGet]
        public HttpResponseMessage GetAllTeams()
        {
            ItemsResponse<WebsiteTeam> response = new ItemsResponse<WebsiteTeam>();

            response.Items = _WebsiteTeamService.GetAllTeams();

            return Request.CreateResponse(response);

        }

        //Test Route
        //[Route("test/{ZipCode}"), HttpGet]
        //public HttpResponseMessage GetTeamIdByZipcode(string ZipCode)
        //{
        //    int TeamId = 0;

        //    TeamId = _WebsiteTeamService.GetTeamIdByZipcode(ZipCode);

        //    if (TeamId == 0)
        //    {
        //        TeamId = 12345;
        //        return Request.CreateResponse(TeamId);

        //    }

        //    return Request.CreateResponse(TeamId);

        //}

    }
}

