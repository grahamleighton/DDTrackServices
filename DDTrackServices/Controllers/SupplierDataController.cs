using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using DDTRACK_DAL.Models;
using DDTRACK_DAL.Models.Common;

namespace DDTrackPlus.Controllers
{
    public class SupplierDataController : CommonDataController
    {
      
        //TODO:Rename to SupplierUser
        private Checks CodeGuard = new Checks();

        public List<UserSuppliers> GetUserSuppliers(string Hash , long userid )
        {

            CodeGuard.HasData(userid, "UserID is missing");
            CodeGuard.HasData(Hash, "Hash is missing");
            Hash h = new Hash(Hash);
            if (!h.isValid())
            {
               throw new DataExpectedException("Hash is invalid");
            }

            clearError();

            List<UserSuppliers> us = new List<UserSuppliers>();
            us.Clear();

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.getUserSuppliers");
                com.Parameters.AddWithValue("@hash", Hash);
                com.Parameters.AddWithValue("@userid", (long)userid);

                if (!sql.sqlFillDataTable(com))
                    throw new Exception(sql.sqlErrorMessage());

                foreach (DataRow dr in sql.sqlDataTable().Rows)
                {
                    UserSuppliers u1 = new UserSuppliers
                    {
                        SupplierCode = Convert.ToString(dr["SupplierCode"]),
                        SupplierName = Convert.ToString(dr["SupplierName"]),
                        SupplierID = Convert.ToInt64(dr["SupplierID"]),
                        UserID = Convert.ToInt64(dr["UserID"])
                    };
                    us.Add(u1);
                }
            }
            catch (Exception ex)
            {
                setError(ex.Message);
                
                us.Clear();
                throw new Exception(ex.Message);
            }

            return us;

        }

        public returnValue DeleteUserSupplier(string hash, long userID, long  supplierID, out int success)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;
            clearError();

            success = 0;


            CodeGuard.HasData(hash, "Hash is missing");
            CodeGuard.HasData(userID, "Userid is missing");
            CodeGuard.HasData(supplierID, "SupplierID is missing");


            Hash h = new Hash(hash);
            if ( !h.isValid() )
            {
                setError("Hash is invalid");
                throw new DataExpectedException("Hash is invalid");
            }


            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.deleteusersupplier");
                com.Parameters.AddWithValue("@hash", hash);
                com.Parameters.AddWithValue("@userid", (long)userID);
                com.Parameters.AddWithValue("@supplierid", supplierID);

                SqlParameter sp = com.Parameters.AddWithValue("@success",0); 
                sp.Direction = ParameterDirection.InputOutput;



                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return 0;
                }
                success = (int)sp.Value;
                                
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

        public returnValue AddUserSupplier(string hash, long userID, long supplierID, string supplierCode , out int success)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;
            clearError();

            success = 0;


            CodeGuard.HasData(hash, "Hash is missing");
            CodeGuard.HasData(userID, "Userid is missing");
            CodeGuard.HasData(supplierID, "SupplierID is missing");


            Hash h = new Hash(hash);
            if (!h.isValid())
            {
                setError("Hash is invalid");
                throw new DataExpectedException("Hash is invalid");
            }


            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.addusersupplier");
                com.Parameters.AddWithValue("@hash", hash);
                com.Parameters.AddWithValue("@userid", (long)userID);
                com.Parameters.AddWithValue("@supplierid", supplierID);
                com.Parameters.AddWithValue("@suppliercode", supplierCode);

                SqlParameter sp = com.Parameters.AddWithValue("@success", 0);
                sp.Direction = ParameterDirection.InputOutput;



                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return 0;
                }
                success = (int)sp.Value;

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
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return 0;
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

        public returnValue loginUser(string UserID, string Password, string Supplier, out string token, string IPAddress, out string change)
        {

            CodeGuard.HasData(UserID, "UserID is missing");
            CodeGuard.HasData(Password, "Password is missing");
            CodeGuard.HasData(Supplier, "Supplier is missing");


            LoginRequest lr = new LoginRequest()
            {
                UserID = UserID,
                Password = Password,
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
                com.Parameters.AddWithValue("@password", req.Password);
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

                    return 0;
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