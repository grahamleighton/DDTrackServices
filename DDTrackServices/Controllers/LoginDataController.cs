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
    public class LoginDataController : CommonDataController
    {
        /*
         * Add Simple Admin to SQL Database used for DDTrack 
         */

        private Checks CodeGuard = new Checks();
        public returnValue AddSimpleAdmin(string UserID)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;
            clearError();

            try
            {
                CodeGuard.HasData(UserID, "UserID Required");
                SqlCommand com = sql.sqlGetCommand("webapi.AddSimpleAdmin");
                com.Parameters.AddWithValue("@username", UserID);
                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    string msg = sql.sqlErrorMessage();
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }
                rc = returnValue.RETURN_SUCCESS;
            }
            catch (DataExpectedException ex)
            {
                setError(ex.Message);
                rc = returnValue.RETURN_DATAEXPECTED;
                throw new DataExpectedException(ex.Message);
            }
            catch (Exception ex)
            {
                setError(ex.Message);
                rc = returnValue.RETURN_FAILURE;
            }

            return rc;

        }

        public returnValue loginUser(string UserID, string Supplier, out string token, string IPAddress, out string change)
        {

            CodeGuard.HasData(UserID, "UserID is missing");
           
            CodeGuard.HasData(Supplier, "Supplier is missing");


            LoginRequest lr = new LoginRequest()
            {
                UserID = UserID,
              
                Supplier = Supplier,
                IPAddress = IPAddress
            };


            return loginUser(lr, out change, out token);
           
        }

        public returnValue loginUser(LoginRequest req,out string changestr,out string token)
        {
            changestr = "";
            token = "";
            
            string c = "";
            Hash t = new Hash();
            

            returnValue rc = returnValue.RETURN_SUCCESS;
            clearError();
            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.loginUser");
                com.Parameters.AddWithValue("@userID", req.UserID);
               
                com.Parameters.AddWithValue("@Supplier", req.Supplier);

                SqlParameter sp = com.Parameters.AddWithValue("@token", t.getHash());
                sp.Direction = ParameterDirection.InputOutput;

                com.Parameters.AddWithValue("@ipaddress", req.IPAddress);

                SqlParameter change = com.Parameters.AddWithValue("@change", c);
                change.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }
                changestr = (string)change.Value;

                if ( t.validHash((string)sp.Value))
                    token = (string)sp.Value;

                 
                rc = returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError( ex.Message );
                rc = returnValue.RETURN_FAILURE;
            }

            return rc;
        }

        public returnValue isUserValid(string Hash , out long UserID )
        {

            returnValue rc = returnValue.RETURN_SUCCESS;
            UserID = 0;

            Hash h = new Hash(Hash);
            
            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.isUserValid");
                com.Parameters.AddWithValue("@hash", h.getHash() );

                SqlParameter sp = com.Parameters.AddWithValue("@UserID", 0);
                sp.Direction = ParameterDirection.InputOutput;


                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return 0;
                }
                UserID = (int)sp.Value;

                rc = returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);
                rc = returnValue.RETURN_FAILURE;
            }

            return rc;
        }

        public returnValue cloneUser(CloneRequest cr, out string clonedhash)
        {
            clonedhash = "";

            Hash crHash = new Hash(cr.Hash);

            returnValue rc = returnValue.RETURN_SUCCESS;
            Hash newHash = new Hash();
            try
            {
                long uid = cr.CloneUserID;
                SqlCommand com = sql.sqlGetCommand("webapi.cloneUser");
                com.Parameters.AddWithValue("@hash", crHash.getHash());
                com.Parameters.AddWithValue("@UserID", uid);

                SqlParameter sp = com.Parameters.AddWithValue("@impersonate", newHash.getHash());
                sp.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }
                if (newHash.validHash((string)sp.Value))
                    clonedhash = (string)sp.Value;

                rc = returnValue.RETURN_SUCCESS;

            }
            catch (Exception ex)
            {
                setError(ex.Message) ;
                rc = returnValue.RETURN_FAILURE;
            }

            return rc;
        }


    }
}