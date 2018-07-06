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
    public class AccountDataController : CommonDataController
    {
      

        private Checks CodeGuard = new Checks();

        public List<string> getMVCRoles() 
        {
            List<string> mr = new List<string>();
            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.getMVCRoles");

                bool resp = sql.sqlFillDataTable(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    throw new Exception(sql.sqlErrorMessage());
                }
                foreach (DataRow dr in sql.sqlDataTable().Rows)
                {
                    mr.Add(Convert.ToString(dr["MVCRole"]));
                }
            }
            catch(Exception ex)
            {
                setError(ex.Message);
                throw new Exception(ex.Message);
            }
            return mr;
        }


    }
}