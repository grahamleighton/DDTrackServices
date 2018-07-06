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
    public class UserDataController : CommonDataController
    {
      

        private Checks CodeGuard = new Checks();


        public returnValue RemoveUser(long userid)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;

            CodeGuard.HasData((long)userid, "Userid is missing");

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.RemoveUser");
                com.Parameters.AddWithValue("@userid", (long)userid);
                SqlParameter sp = com.Parameters.AddWithValue("@success", 0);
                sp.Direction = ParameterDirection.InputOutput;


                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }

                if ( (int)sp.Value == 1)
                {
                    return returnValue.RETURN_FAILURE;
                }

                return returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);

                throw new Exception(ex.Message);

            }
        }
        public returnValue AddSimpleAdmin(string UserName )
        {
            
            CodeGuard.HasData(UserName, "UserName is missing");
            clearError();

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.addSimpleAdmin");
                com.Parameters.AddWithValue("@username", UserName);

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());

                    return returnValue.RETURN_FAILURE;
                }

                return returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);
                
                throw new Exception(ex.Message);
               
            }

           
        }

        public returnValue DeActivateUser(string hash,long userid,out DeActivateUserResponse success)
        {
            
            CodeGuard.HasData(hash, "hash is missing");
            CodeGuard.HasData(userid, "userid is missing");

            Hash h = new Hash(hash);
            if (!h.isValid())
            {
                throw new DataExpectedException("Hash is invalid");
            }


            clearError();
            success = DeActivateUserResponse.RETURN_SUCCESS;

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.deactivateUser");
                com.Parameters.AddWithValue("@userid", (long)userid);
                com.Parameters.AddWithValue("@hash", hash);
                SqlParameter sp = com.Parameters.AddWithValue("@success", 0);
                sp.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());
                    success = DeActivateUserResponse.RETURN_FAIL;
                    return returnValue.RETURN_FAILURE;
                }
                success = (DeActivateUserResponse) sp.Value;

                return returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);
                success = DeActivateUserResponse.RETURN_FAIL;

                throw new Exception(ex.Message);
            }
           
        }


        public returnValue ActivateUser(string hash, long userid, out ActivateUserResponse success)
        {

            CodeGuard.HasData(hash, "hash is missing");
            CodeGuard.HasData(userid, "userid is missing");

            Hash h = new Hash(hash);
            if (!h.isValid())
            {
                throw new DataExpectedException("Hash is invalid");
            }



            clearError();
            success = (int)ActivateUserResponse.RETURN_SUCCESS;

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.activateUser");
                com.Parameters.AddWithValue("@userid", (long)userid);
                com.Parameters.AddWithValue("@hash", hash);
                SqlParameter sp = com.Parameters.AddWithValue("@success", 0);
                sp.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());
                    success = ActivateUserResponse.RETURN_FAIL;
                    return returnValue.RETURN_FAILURE;
                }
                success = (ActivateUserResponse)sp.Value;

                return returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);
                success = ActivateUserResponse.RETURN_FAIL;

                throw new Exception(ex.Message);
            }

        }

        public List<Users> GetUsers(string hash, string search,out int count)
        {
            count = 0;
            CodeGuard.HasData(hash, "Hash is missing");

            Hash h = new Hash(hash);
            if (!h.isValid())
            {
                throw new DataExpectedException("Hash is invalid");
            }

            clearError();

            List<Users> us = new List<Users>();
            us.Clear();
            DataRow dr2;
            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.getUsers");
                com.Parameters.AddWithValue("@hash", hash);
                
              
                SqlParameter sp = com.Parameters.AddWithValue("@count", 0);
                sp.Direction = ParameterDirection.InputOutput;

                com.Parameters.AddWithValue("@search", search);

                bool resp = sql.sqlFillDataTable(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());
                    count = 0;
                    throw new Exception(sql.sqlErrorMessage());
                }
                foreach (DataRow dr in sql.sqlDataTable().Rows)
                {
                    dr2 = dr;
                    Users u = new Users
                    {

                    //    SupplierCode = CodeGuard.GetDefaultFor(Convert.ToString(dr["SupplierCode"]))
                        SupplierCode = Convert.ToString(CodeGuard.GetDefaultForDB(dr["SupplierCode"],""))
                        , UserID = Convert.ToInt64(CodeGuard.GetDefaultForDB(dr["UserID"],0))
                        , UserName = Convert.ToString(CodeGuard.GetDefaultForDB(dr["UserName"],""))
                        , AdminLevel = Convert.ToInt32(CodeGuard.GetDefaultForDB(dr["AdminLevel"],0))
                        , Password = ""
                        , UID = Convert.ToString(CodeGuard.GetDefaultForDB(dr["UID"],""))
                        , CreatedOn = Convert.ToDateTime(CodeGuard.GetDefaultForDB(dr["CreatedOn"],new DateTime(1980,1,1)))
                        ,FullName = Convert.ToString(CodeGuard.GetDefaultForDB(dr["FullName"],""))
                        ,LastPasswordChange = Convert.ToDateTime(CodeGuard.GetDefaultForDB(dr["LastPasswordChange"], new DateTime(1980, 1, 1)))
                        ,UnsuccessfulLogonCount = Convert.ToInt32(CodeGuard.GetDefaultForDB(dr["UnsuccessfulLogonCount"],0))
                        ,SecurityQuestionID = Convert.ToInt32(CodeGuard.GetDefaultForDB(dr["SecurityQuestionID"],0))
                        ,SecurityAnswer = Convert.ToString(CodeGuard.GetDefaultForDB(dr["SecurityAnswer"],""))
                        ,LastLoginTime = Convert.ToDateTime(CodeGuard.GetDefaultForDB(dr["LastLoginTime"], new DateTime(1980, 1, 1)))
                        ,Deactivated = Convert.ToBoolean(CodeGuard.GetDefaultForDB(dr["Deactivated"],false))
                        ,Telephone = Convert.ToString(CodeGuard.GetDefaultForDB(dr["Telephone"],""))
                        ,SupplierName = Convert.ToString(CodeGuard.GetDefaultForDB(dr["SupplierName"],""))
                    };
                    us.Add(u);
                    count++;
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

        public List<AdminLevels> GetCreateAdminLevels(string hash, long userid)
        {
            
            CodeGuard.HasData(hash, "Hash is missing");

            Hash h = new Hash(hash);
            if (!h.isValid())
            {
                throw new DataExpectedException("Hash is invalid");
            }

            clearError();

            List<AdminLevels> us = new List<AdminLevels>();
            us.Clear();

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.getAdminLevelsCreate");
                com.Parameters.AddWithValue("@hash", hash);
                com.Parameters.AddWithValue("@userID", (long)userid );

                bool resp = sql.sqlFillDataTable(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());
                    throw new Exception(sql.sqlErrorMessage());
                }
                foreach (DataRow dr in sql.sqlDataTable().Rows)
                {
                    AdminLevels al = new AdminLevels
                    {
                        AdminName = CodeGuard.GetDefaultFor(Convert.ToString(dr["AdminLevelName"])),
                        AdminNumber = CodeGuard.GetDefaultFor(Convert.ToInt32(dr["AdminLevelNumber"])) 
                    };
                    us.Add(al);
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
        public List<AdminLevelRoles> GetAdminLevels(string hash)
        {

            CodeGuard.HasData(hash, "Hash is missing");

            Hash h = new Hash(hash);
            if (!h.isValid())
            {
                throw new DataExpectedException("Hash is invalid");
            }

            clearError();

            List<AdminLevelRoles> us = new List<AdminLevelRoles>();
            us.Clear();

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.getAdminLevels");
                com.Parameters.AddWithValue("@hash", hash);
               

                if ( !sql.sqlFillDataTable(com))
                {
                    throw new Exception(sql.sqlErrorMessage());
                }
                foreach (DataRow dr in sql.sqlDataTable().Rows)
                {
                    AdminLevelRoles al = new AdminLevelRoles
                    {
                        AdminLevelName = CodeGuard.GetDefaultFor(Convert.ToString(dr["AdminLevelName"])),
                        AdminLevelNumber = CodeGuard.GetDefaultFor(Convert.ToInt32(dr["AdminLevelNumber"])),
                        MVCRole = CodeGuard.GetDefaultFor(Convert.ToString(dr["MVCRole"])),

                    };
                    us.Add(al);
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

        public returnValue GetUser(string hash , long userid , out User u)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;


            CodeGuard.HasData(hash, "Hash is missing");


            Hash h = new Hash(hash);
            if (!h.isValid())
            {
                u = new User();
                throw new DataExpectedException("Hash is invalid");
            }

            clearError();

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.getUser");
                com.Parameters.AddWithValue("@hash", hash);
                com.Parameters.AddWithValue("@userID", (long)userid);

                if (!sql.sqlFillDataTable(com))
                    throw new Exception(sql.sqlErrorMessage());

                foreach (DataRow dr in sql.sqlDataTable().Rows)
                {
                    u = new User
                    {
                        UserID = CodeGuard.GetDefaultFor(Convert.ToInt64(dr["UserID"]))
                        ,
                        UserName = CodeGuard.GetDefaultFor(Convert.ToString(dr["UserName"]))
                        ,
                        AdminLevel = CodeGuard.GetDefaultFor(Convert.ToInt32(dr["AdminLevel"]))
                        ,
                        CreatedOn = CodeGuard.GetDefaultFor(Convert.ToDateTime(dr["CreatedOn"]))
                        ,
                        FullName = CodeGuard.GetDefaultFor(Convert.ToString(dr["FullName"]))
                        ,
                        Deactivated = CodeGuard.GetDefaultFor(Convert.ToInt32(dr["Deactivated"]))
                        ,
                        Telephone = CodeGuard.GetDefaultFor(Convert.ToString(dr["Telephone"]))
                        ,AdminLevelName = CodeGuard.GetDefaultFor(Convert.ToString(dr["AdminLevelName"]))
                        ,
                        SecurityQuestion = CodeGuard.GetDefaultFor(Convert.ToString(dr["Question"]))
                        ,SecurityQuestionID = CodeGuard.GetDefaultFor(Convert.ToInt32(dr["SecurityQuestionID"]))
                    };
                    return returnValue.RETURN_SUCCESS;

                }
                u = new User();
                return returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);

                throw new Exception(ex.Message);
            }
            
        }
        public returnValue AddUser(string hash, string userName, string supplierCode, string fullName, string telephone, int adminLevel, out AddUserResponseEnum rsp, out string errorMessage)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;

            rsp = AddUserResponseEnum.RETURN_SUCCESS;
            errorMessage = "";

            CodeGuard.HasData(hash, "Hash is missing");
            Hash h = new Hash(hash);
            if (!h.isValid())
            {
                throw new DataExpectedException("Hash is invalid");
            }
            CodeGuard.HasData(userName, "UserName is missing");

            clearError();

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.addUser");
                com.Parameters.AddWithValue("@hash", hash);
                com.Parameters.AddWithValue("@username", userName);
                com.Parameters.AddWithValue("@suppliercode", supplierCode);
                com.Parameters.AddWithValue("@fullname", fullName);
                com.Parameters.AddWithValue("@telephone", telephone);
                com.Parameters.AddWithValue("@adminlevel", adminLevel);

                SqlParameter sp_rc = com.Parameters.AddWithValue("@rc", 0);
                sp_rc.Direction = ParameterDirection.InputOutput;
                SqlParameter ErrMsg = com.Parameters.AddWithValue("@errormessage", "");
                ErrMsg.Direction = ParameterDirection.InputOutput;
                ErrMsg.Size = 4000;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());
                    rsp = AddUserResponseEnum.RETURN_FAILED;
                    errorMessage = (string)ErrMsg.Value;
                    return returnValue.RETURN_FAILURE;
                }

                rsp = (AddUserResponseEnum)sp_rc.Value;
                errorMessage = (string)ErrMsg.Value;

                return returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);

                throw new Exception(ex.Message);
            }

        }

        public returnValue UpdateUser(string hash, long userID, string userName, string fullName, string telephone, int adminLevel, out AddUserResponseEnum rsp, out string errorMessage)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;

            rsp = AddUserResponseEnum.RETURN_SUCCESS;
            errorMessage = "";

            CodeGuard.HasData(hash, "Hash is missing");
            Hash h = new Hash(hash);
            if (!h.isValid())
            {
                throw new DataExpectedException("Hash is invalid");
            }
            CodeGuard.HasData(userName, "UserName is missing");

            clearError();

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.UpdateUser");
                com.Parameters.AddWithValue("@hash", hash);
                com.Parameters.AddWithValue("@userid", (long)userID);

                com.Parameters.AddWithValue("@username", userName);
               
                com.Parameters.AddWithValue("@fullname", fullName);
                com.Parameters.AddWithValue("@telephone", telephone);
                com.Parameters.AddWithValue("@adminlevel", adminLevel);

                SqlParameter sp_rc = com.Parameters.AddWithValue("@rc", 0);
                sp_rc.Direction = ParameterDirection.InputOutput;
                SqlParameter ErrMsg = com.Parameters.AddWithValue("@errormessage", "");
                ErrMsg.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());
                    rsp = AddUserResponseEnum.RETURN_FAILED;
                    errorMessage = (string)ErrMsg.Value;
                    return returnValue.RETURN_FAILURE;
                }

                rsp = (AddUserResponseEnum)sp_rc.Value;
                errorMessage = (string)ErrMsg.Value;

                return returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);

                throw new Exception(ex.Message);
            }

        }

        public returnValue GetMyUserDetails(string hash, out bool ValidUser , out MyUserDetails u)
        {
            returnValue rc = returnValue.RETURN_SUCCESS;

            ValidUser = false;
            CodeGuard.HasData(hash, "Hash is missing");


            Hash h = new Hash(hash);
            if (!h.isValid())
            {
              
                throw new DataExpectedException("Hash is invalid");
            }

            clearError();

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.getMyUserDetails");
                com.Parameters.AddWithValue("@hash", hash);
                SqlParameter sp = com.Parameters.AddWithValue("@validuser", 0);
                sp.Direction = ParameterDirection.InputOutput;

                bool resp = sql.sqlFillDataTable(com);
                if ( !resp)
                {
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());
                    u = new MyUserDetails();
                    return returnValue.RETURN_FAILURE;
                }
                foreach (DataRow dr in sql.sqlDataTable().Rows)
                {
                    u = new MyUserDetails
                    {
                        UserID = CodeGuard.GetDefaultFor(Convert.ToInt64(dr["UserID"]))
                        ,
                        UserName = CodeGuard.GetDefaultFor(Convert.ToString(dr["UserName"]))
                        ,
                        AdminLevel = CodeGuard.GetDefaultFor(Convert.ToInt32(dr["AdminLevel"]))
                        ,
                        MVCRole = CodeGuard.GetDefaultFor(Convert.ToString(dr["MVCRole"]))
                        ,
                        FullName = CodeGuard.GetDefaultFor(Convert.ToString(dr["FullName"]))
                        ,
                        SupplierCode = CodeGuard.GetDefaultFor(Convert.ToString(dr["SupplierCode"]))
                        ,
                        SupplierName = CodeGuard.GetDefaultFor(Convert.ToString(dr["SupplierName"]))
                        ,
                        Deactivated = CodeGuard.GetDefaultFor(Convert.ToBoolean(dr["Deactivated"]))
                        ,
                        Telephone = CodeGuard.GetDefaultFor(Convert.ToString(dr["Telephone"]))
                        ,
                        AdminLevelName = CodeGuard.GetDefaultFor(Convert.ToString(dr["AdminLevelName"]))
                        
                    };
                    ValidUser = true;
                    return returnValue.RETURN_SUCCESS;

                }
                u = new MyUserDetails();
                ValidUser = true;
                return returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);

                throw new Exception(ex.Message);
            }

        }



        public List<AuditUserDetails> GetAuditUserDetails(string hash, out int count ,string SupplierCode,string User , string DateFrom , string DateTo )
        {
            count = 0;
            CodeGuard.HasData(hash, "Hash is missing");

            Hash h = new Hash(hash);
            if (!h.isValid())
            {
                throw new DataExpectedException("Hash is invalid");
            }

            clearError();

            List<AuditUserDetails> us = new List<AuditUserDetails>();
            us.Clear();

            try
            {
                SqlCommand com = sql.sqlGetCommand("webapi.getAuditUserDetails");
                com.Parameters.AddWithValue("@hash", hash);

                SqlParameter sp = com.Parameters.AddWithValue("@count", 0);
                sp.Direction = ParameterDirection.InputOutput;

                com.Parameters.AddWithValue("@suppliercode", SupplierCode);
                com.Parameters.AddWithValue("@user", User);
                com.Parameters.AddWithValue("@datefrom", DateFrom);
                com.Parameters.AddWithValue("@dateto", DateTo);



                if (!sql.sqlFillDataTable(com))
                    throw new Exception(sql.sqlErrorMessage());

                foreach (DataRow dr in sql.sqlDataTable().Rows)
                {
                    AuditUserDetails u = new AuditUserDetails
                    {
                        AffectedUserID = CodeGuard.GetDefaultFor(Convert.ToInt32(dr["AffectedUserID"])),
                        AuditID = CodeGuard.GetDefaultFor(Convert.ToInt32(dr["AuditID"])),
                        UserID = CodeGuard.GetDefaultFor(Convert.ToInt32(dr["UserID"])),
                        EventDescription = CodeGuard.GetDefaultFor(Convert.ToString(dr["EventDescription"])),
                        By = CodeGuard.GetDefaultFor(Convert.ToString(dr["By"])),
                        For = CodeGuard.GetDefaultFor(Convert.ToString(dr["For"])),
                        ForSupplierCode = CodeGuard.GetDefaultFor(Convert.ToString(dr["ForSupplierCode"])),
                        ForSupplierName = CodeGuard.GetDefaultFor(Convert.ToString(dr["ForSupplierName"])),
                        BySupplierCode = CodeGuard.GetDefaultFor(Convert.ToString(dr["BySupplierCode"])),
                        BySupplierName = CodeGuard.GetDefaultFor(Convert.ToString(dr["BySupplierName"])),
                        AuditTimeStamp = CodeGuard.GetDefaultFor(Convert.ToDateTime(dr["AuditTimeStamp"]))
                    };
                    us.Add(u);
                    count++;
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

    }
}