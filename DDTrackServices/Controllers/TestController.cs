using DDTRACK_DAL.Models;
using DDTRACK_DAL.Models.Common;
using DDTrackPlus.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace DDTRACK_DAL.Controllers
{
    [Authorize]
    public class TestController : ApiController
    {
   //     private DDTrackEntities db = new DDTrackEntities();
        private TestDataController dc = new TestDataController();


        [ResponseType(typeof(ConnectTest))]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult Connect(string apikey = "" )
        {
            ConnectTest conntest = new ConnectTest();

            try
            {
                conntest.Env = ConfigurationManager.AppSettings["environment"];
            }
            catch(Exception )
            {
                conntest.Env = "unknown";
            }

            try
            {
                conntest.Service = "OK";
            }
            catch(Exception e)
            {
                if ( apikey == "hstd004")
                    conntest.Service = e.Message;
                else
                    conntest.Service = "Failed";
            }
            try
            {
                string ErrMsg = "";
                returnValue rv = dc.testDatabaseConnect(apikey, out ErrMsg);
                conntest.Data = "OK";

            }
            catch (Exception e)
            {
                if (apikey == "hstd004")
                    conntest.Data = e.Message;
                else
                    conntest.Data = "Failed";
            }

            return Ok(conntest);
        }


        /*
        [ResponseType(typeof(bool))]
        public IHttpActionResult ClearTestData(string AuthKey)
        {
            if (AuthKey != ConfigurationManager.AppSettings["ApiKey"])
                return Ok(false);
            db.ClearTestData();
            return Ok(true);
        }

    */ 
    }
}
