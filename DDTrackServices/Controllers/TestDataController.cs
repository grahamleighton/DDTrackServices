using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DDTrackSQL;
using System.Data;
using DDTrackSQL.Controllers;
using DDTRACK_DAL.Models;
using DDTRACK_DAL.Models.Common;

namespace DDTrackPlus.Controllers
{
    public class TestDataController : CommonDataController
    {
        private Checks CodeGuard = new Checks();
//        User u = db.getUser(sp.Hash, sp.UserID).SingleOrDefault();

      
        public returnValue testDatabaseConnect(string apikey , out string ErrMsg)
        {
            ErrMsg = "OK";
            try
            {
                bool result  = sql.sqlConnectTest();
                if (result)
                    return returnValue.RETURN_SUCCESS;
                else
                    return returnValue.RETURN_FAILURE;
            }
            catch(Exception ex )
            {
                if (apikey == "hstd004")
                    ErrMsg = ex.Message;

                return returnValue.RETURN_FAILURE;
            }

        }
    }
}