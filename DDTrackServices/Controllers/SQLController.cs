using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DDTrackSQL.Controllers
{
    public class SQLController : Controller
    {
        private static string _connectionString;
        private static SqlConnection _conn;
        private static string em = "";
        private DataTable _myDataTable = null;

        public SQLController(string connectionstring)
        {
            _connectionString = connectionstring;
        }

        public bool sqlConnectTest()
        {
            bool result = false;

            try
            {
                _conn.Open();
                return true;
            }
            catch(Exception ex)
            {
                em = ex.Message;
                throw new Exception(em);
            }

            

            return result;
        }

        public DataTable sqlDataTable()
        {
            return _myDataTable;
        }
        public bool sqlError()
        {
            return !String.IsNullOrEmpty(em);
        }
        public string sqlErrorMessage()
        {
            return em;
        }

        private bool _sqlCreateDbConnection()
        {
            bool result = false;
            em = "";
            try
            {
                _conn = new SqlConnection(_connectionString);
                result = true;
            }
            catch (Exception ex)
            {
                em = ex.Message;
                result = false;
            }

            return result;
        }
        public SqlCommand sqlGetCommand(string StoredProc)
        {
            em = "";
            try
            {
                bool res = _sqlCreateDbConnection();
                if (!res)
                {
                    return null;
                }
                SqlCommand comm = new SqlCommand(StoredProc, _conn);
                comm.CommandType = CommandType.StoredProcedure;
                return comm;
            }
            catch (Exception ex)
            {
                em = ex.Message;
            }

            return null;

        }

        public bool sqlFillDataTable(SqlCommand cmd)
        {
            try
            {


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                if (_myDataTable != null)
                    _myDataTable.Dispose();
                _myDataTable = new DataTable();


                _conn.Open();
                da.Fill(_myDataTable);
                _conn.Close();
            }
            catch (Exception ex)
            {
                em = ex.Message;
                return false;
            }

            return true;
        }

        public bool sqlFillDataTable(SqlCommand cmd,DataTable dt)
        {
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            _conn.Open();
            da.Fill(dt);
            _conn.Close();

            return true;
        }


        public bool sqlRunCommand(SqlCommand cmd)
        {
            try
            {
                _conn.Open();
                cmd.ExecuteNonQuery();
                _conn.Close();
            }
            catch (Exception ex)
            {
                em = ex.Message;
                return false;
            }
            return true;
        }
    }
}