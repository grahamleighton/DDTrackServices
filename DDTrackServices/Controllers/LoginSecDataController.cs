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
    public class LoginSecDataController : CommonDataController
    {
        private Checks CodeGuard = new Checks();
//        User u = db.getUser(sp.Hash, sp.UserID).SingleOrDefault();

      
        public returnValue setPasswordForUser(string username)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;
            clearError();

            try
            {
                CodeGuard.HasData(username, "Username is missing");
                SqlCommand com = sql.sqlGetCommand("webapi.setPasswordForUser");
                com.Parameters.AddWithValue("@username", username);

                SqlParameter spSuccess = com.Parameters.AddWithValue("@success", 0);
                spSuccess.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }

                CodeGuard.IsValid((int)spSuccess.Value, "Success value not valid");


                if ((int)spSuccess.Value == 1)
                    rc = returnValue.RETURN_SUCCESS;
                else
                {
                    rc = returnValue.RETURN_FAILURE;
                }

            }
            catch (Exception ex)
            {

            }



            return rc;
        }

        public returnValue setPasswordChange(string Hash, long UserID )
        {
            returnValue rc = returnValue.RETURN_SUCCESS;

            Hash h = new Hash(Hash);

            if (!h.isValid())
            {
                return returnValue.RETURN_INVALIDARGS;
            }


            clearError();
            try
            {
                CodeGuard.HasData(UserID, "UserID is invalid");
                SqlCommand com = sql.sqlGetCommand("webapi.setPasswordChange");
                com.Parameters.AddWithValue("@hash", h.getHash());
                com.Parameters.AddWithValue("@UserID", (long)UserID);

                SqlParameter spSuccess = com.Parameters.AddWithValue("@success", 0);
                spSuccess.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }

                CodeGuard.IsValid((int)spSuccess.Value,"Success value not valid");


                if ((int)spSuccess.Value == 1)
                    rc = returnValue.RETURN_SUCCESS;
                else
                {
                    rc = returnValue.RETURN_FAILURE;
                }
            }
            catch (DataException ex)
            {
                setError(ex.Message);
                throw new DataExpectedException(ex.Message);
            }
            catch (Exception ex)
            {
                setError(ex.Message);
                rc = returnValue.RETURN_FAILURE;
            }


            return rc;
        }

        public returnValue setPasswordandSecurityQuestion(string Hash, long UserID, string Password, long SecurityQuestionID, string SecurityAnswer)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;

            Hash h = new Hash(Hash);

            if (!h.isValid())
            {
                return returnValue.RETURN_INVALIDARGS;
            }
            
            if ( String.IsNullOrEmpty(Password))
            {
                return returnValue.RETURN_DATAEXPECTED;
            }

            clearError();
            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.setPasswordandSecurityQuestion");

                com.Parameters.AddWithValue("@hash", h.getHash());
                com.Parameters.AddWithValue("@UserID", (long)UserID);
                com.Parameters.AddWithValue("@Password", Password);
                com.Parameters.AddWithValue("@SecurityQuestionID", SecurityQuestionID);
                com.Parameters.AddWithValue("@SecurityAnswer", SecurityAnswer);

                SqlParameter sp = com.Parameters.AddWithValue("@success", 0);
                sp.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }
                if ((int)sp.Value == 1)
                    rc = returnValue.RETURN_SUCCESS;
                else
                {
                    rc = returnValue.RETURN_FAILURE;
                }
            }
            catch (Exception ex)
            {
                setError(ex.Message);
                rc = returnValue.RETURN_FAILURE;
            }


            return rc;
        }

        public returnValue getSecurityQuestion(long ?UserID, out string question)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;

            question = "";

            CodeGuard.HasData(UserID, "UserID is missing");

            clearError();
            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.getSecurityQuestion");

                com.Parameters.AddWithValue("@UserID", (long)UserID);

                SqlParameter sp = com.Parameters.AddWithValue("@question", "");
                sp.Direction = ParameterDirection.InputOutput;
                sp.Size = 600;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }
                question = (string)sp.Value;


                rc = returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);
                
                rc = returnValue.RETURN_FAILURE;
            }


            return rc;

        }

        public returnValue validateRecovery(string Hash, out long userid, out int valid, out string suppliercode)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;
            userid = 0;
            valid = 0;
            suppliercode = "";
            Hash h = new Hash(Hash);

            if (!h.isValid())
            {
                return returnValue.RETURN_INVALIDARGS;
            }

            SupplierCode sc = new SupplierCode(suppliercode);

            clearError();
            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.validateRecovery");

                com.Parameters.AddWithValue("@hash", h.Value );

                SqlParameter spUserid =  com.Parameters.AddWithValue("@userid", (long)userid);
                spUserid.Direction = ParameterDirection.InputOutput;

                SqlParameter spValid = com.Parameters.AddWithValue("@valid", valid);
                spValid.Direction = ParameterDirection.InputOutput;

                SqlParameter spSupplier = com.Parameters.AddWithValue("@suppliercode", sc.Value);
                spSupplier.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }
                userid = (long)spUserid.Value;
                valid = (int)spValid.Value;
                suppliercode = (string)spSupplier.Value;

                rc = returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);

                rc = returnValue.RETURN_FAILURE;
            }

            return rc;

        }

        public returnValue isEmailAvailable(string Hash, string UserName, out bool exists)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;
            exists = true;

            Hash h = new Hash(Hash);

            if (!h.isValid())
            {
                return returnValue.RETURN_INVALIDARGS;
            }


            clearError();
            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.isEmailAvailable");

                com.Parameters.AddWithValue("@hash", h.Value);
                com.Parameters.AddWithValue("@username", UserName );

                SqlParameter spExists = com.Parameters.AddWithValue("@exists", false);
                spExists.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }

                exists  = (bool)spExists.Value;

                rc = returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);

                rc = returnValue.RETURN_FAILURE;
            }


            return rc;

        }

        public returnValue isSupplierAvailable(string Hash, string SupplierCode, out bool exists)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;
            exists = true;

            CodeGuard.HasData(Hash, "Hash is missing");
            CodeGuard.HasData(SupplierCode, "SupplierCode is missing");

            Hash h = new Hash(Hash);

            if (!h.isValid())
            {
                return returnValue.RETURN_INVALIDARGS;
            }


            SupplierCode sc = new SupplierCode(SupplierCode);
            if (!sc.ValidSupplier(SupplierCode))
            {
                return returnValue.RETURN_INVALIDARGS;
            }
            
             

            clearError();
            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.isSupplierAvailable");

                com.Parameters.AddWithValue("@hash", h.Value);
                com.Parameters.AddWithValue("@suppliercode", sc.Value);

                SqlParameter spExists = com.Parameters.AddWithValue("@exists", false);
                spExists.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }

                exists = (bool)spExists.Value;

                rc = returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);

                rc = returnValue.RETURN_FAILURE;
            }


            return rc;

        }

        public returnValue  checkSecurityAnswer(string Hash, long ?UserID,  string Answer, out int correct)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;

            correct = 1;


            CodeGuard.HasData(Hash,"Hash is missing");
            CodeGuard.HasData(UserID, "UserID is missing");
            CodeGuard.HasData(Answer, "Answer is missing");




            Hash h = new Hash(Hash);

            if (!h.isValid())
            {
                return returnValue.RETURN_INVALIDARGS;
            }

            clearError();
            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.checkSecurityAnswer");

                com.Parameters.AddWithValue("@hash", h.Value);
                com.Parameters.AddWithValue("@userid", (long)UserID);
                com.Parameters.AddWithValue("@answer", Answer);

                SqlParameter spCorrect = com.Parameters.AddWithValue("@correct",0 );
                spCorrect.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }

                correct = (int)spCorrect.Value;

                rc = returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);

                rc = returnValue.RETURN_FAILURE;
            }



            return rc;

        }


        public returnValue getSecurityQuestions(string hash, out List<SecurityQuestions> qs)
        {
            qs = new List<SecurityQuestions>();

            // check for special case of hash = "Expired"
            if (hash.ToUpper() != "EXPIRED")
            {

                Hash h = new Hash(hash);

                if (!h.isValid())
                {
                    return returnValue.RETURN_INVALIDARGS;
                }
            }

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.getSecurityQuestions");
                com.Parameters.AddWithValue("@hash", hash.ToUpper());

                bool resp = sql.sqlFillDataTable(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }

                foreach (DataRow dr in sql.sqlDataTable().Rows)
                {
                    SecurityQuestions sq = new SecurityQuestions
                    {
                        Question = (dr["Question"] == null) ? "" : Convert.ToString(dr["Question"]),
                        Id = Convert.ToInt32(dr["Id"])
                    };
                    qs.Add(sq);
                }
            }
            catch (Exception ex)
            {
                setError( ex.Message);
                return returnValue.RETURN_FAILURE;
            }

            return returnValue.RETURN_SUCCESS;

        }

        public returnValue getRecoveryUser(string UserName, string SupplierCode, out long userid)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;
            userid = 0;

            SupplierCode sc = new SupplierCode(SupplierCode);
            if (!sc.ValidSupplier(SupplierCode))
            {
                return returnValue.RETURN_INVALIDARGS;
            }

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.getRecoveryUser");
                com.Parameters.AddWithValue("@username", UserName);
                com.Parameters.AddWithValue("@supplier", sc.Value);

                SqlParameter spUserID = com.Parameters.AddWithValue("@userid", 0);
                spUserID.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }

                userid = (int)spUserID.Value;

            }
            catch (Exception ex)
            {
                setError(ex.Message);
            }

            return rc;

        }

        public returnValue getRecoveryEmail(long userid, out string body, out string subject, out string emailaddress)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;
            body = "";
            subject = "";
            emailaddress = "";

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.getRecoveryEmail");
                com.Parameters.AddWithValue("@userid", (long)userid);

                SqlParameter spBody = com.Parameters.AddWithValue("@body", "");
                spBody.Size = 4000;
                spBody.Direction = ParameterDirection.InputOutput;
                SqlParameter spSubject = com.Parameters.AddWithValue("@subject", "");
                spSubject.Direction = ParameterDirection.InputOutput;
                spSubject.Size = 200;
                SqlParameter spEmailAddress = com.Parameters.AddWithValue("@emailaddress", "");
                spEmailAddress.Size = 150;
                spEmailAddress.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }

                body = (string)spBody.Value;
                subject = (string)spSubject.Value;
                emailaddress = (string)spEmailAddress.Value;
                 
            }
            catch (Exception ex)
            {
                setError(ex.Message);
            }


            return rc;

        }


        /*
         * 
         * 
         *     public ParcelDetails getParcelDetails(string parcel )
                {
                    ParcelDetails pd = new ParcelDetails();

                    try
                    {
                        SqlCommand com = sql.sqlGetCommand("getParcelDetails");
                        com.Parameters.AddWithValue("@Barcode", parcel);

                        sql.sqlFillDataTable(com);

                        foreach ( DataRow dr in sql.sqlDataTable().Rows )
                        {
                            ParcelDetails pd2 = new ParcelDetails
                            {
                                Agent = ( dr["Agent"] == null ) ? "" : Convert.ToString(dr["Agent"])  ,
                                AgentName = (dr["AgentName"] == null ) ? "" :  Convert.ToString(dr["AgentName"]),
                                Barcode = (dr["Barcode"] == null ) ? "" : Convert.ToString(dr["Barcode"]),
                                DateOrdered = Convert.ToDateTime(dr["DateOrdered"]),
                                CollectedBy = ( dr["CollectedBy"] == null ) ? "" : Convert.ToString(dr["CollectedBy"]),
                                ScannedIn = Convert.ToDateTime(dr["ScannedIn"]),
                                ScannedOut = Convert.ToDateTime(dr["ScannedOut"]),
                                FirstScannedIn = Convert.ToDateTime(dr["FirstScannedIn"]),
                                FirstScannedOut = Convert.ToDateTime(dr["FirstScannedOut"])
                            };
                            return pd2;
                        }
                    }
                   catch(Exception ex)
                    {
                        em = ex.Message;

                    }

                    return pd;
                }
         * 
         * 
         * 
         */



    }
}