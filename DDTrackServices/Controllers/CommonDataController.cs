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

namespace DDTrackPlus.Controllers
{
    public class CommonDataController : Controller
    {
        // GET: Data
        public SQLController sql = new SQLController(ConfigurationManager.AppSettings["DDTrackPlusDBConn"]);

        private static string em = "";

     

        /*
         *  Error related functions
         *
         *
         * 
         */

        public string getError()
        {
            return em;
        }
        public string getLastError()
        {
            return getError();
        }
        public bool Error()
        {
            return !String.IsNullOrEmpty(em);
        }
        public void clearError()
        {
            em = "";
        }
        public void setError(string errorMessage)
        {
            em = errorMessage;
        }

     
    }
}