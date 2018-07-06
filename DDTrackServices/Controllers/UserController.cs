using DDTRACK_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Data.Entity.Core.Objects;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Configuration;
using DDTrackPlus.Controllers;
using DDTRACK_DAL.Models.Common;

namespace DDTRACK_DAL.Controllers
{
   [Authorize]

   
    public class UserController : ApiController
    {
    //    private DDTrackEntities db = new DDTrackEntities();
        private UserDataController uc = new UserDataController();


        private enum ActivationStatus { OK, FAIL, NOTAUTH, USERNOTEXIST, INVPARAM };

        [ResponseType(typeof(bool))]
        [AllowAnonymous]
      
        public IHttpActionResult AddSimpleUser( string username, string apikey)
        {
            if ( apikey != ConfigurationManager.AppSettings["ApiKey"])
            {
                return Ok(false);
            }
            try
            {
//                db.AddSimpleAdmin(username);
                returnValue rc = uc.AddSimpleAdmin(username);
                if (rc != returnValue.RETURN_SUCCESS)
                {

                }
            }
            catch (DataExpectedException )
            {
            }
            catch (NullReferenceException )
            {
            }
            catch (Exception )
            {
            }



            return Ok(true);
        }

      


        [ResponseType(typeof(string))]
        [HttpGet]
        public IHttpActionResult DeActivate(string hash, long UserID)
        { 
            DeActivateUserResponse success = DeActivateUserResponse.RETURN_SUCCESS;
            //     ObjectParameter success = new ObjectParameter("success", typeof(int));
            //db.deactivateUser(UserID, hash, success);
          
            try
            {
                long UID = UserID;

                returnValue rc = uc.DeActivateUser(hash, UID, out success);
                
                if ( rc == returnValue.RETURN_SUCCESS )
                {

                    if ((DeActivateUserResponse)success != DeActivateUserResponse.RETURN_SUCCESS)
                    {
                        switch ((DeActivateUserResponse)success)
                        {
                            case DeActivateUserResponse.RETURN_FAIL:
                                {
                                    return BadRequest("General Failure");
//                                    setError("General Failure");
                                }
                            case DeActivateUserResponse.RETURN_INVALIDPARAM:
                                {
                                    return BadRequest("Invalid parameters");
                                }
                            case DeActivateUserResponse.RETURN_NOTAUTHORIZED:
                                {
                                    return BadRequest("User Not Authorized");
                                }
                            case DeActivateUserResponse.RETURN_USERNOTEXIST:
                                {
                                    return BadRequest("User does not exist");
                                }
                            default: break;
                        }
                    }

                }
                else
                {
                    return BadRequest(uc.getError());
                }
            }
            catch (DataExpectedException ex)
            {
                return BadRequest("General error");
//                return setError(ex.Message);
            }
            catch (NullReferenceException )
            {
                return BadRequest("Null Reference");
                //return setError("NullReference");
            }
            catch (Exception )
            {
                return BadRequest("General error");
                //return setError("General Failure");
            }

            return Ok("");
        }

        [ResponseType(typeof(string))]
        [HttpGet]
        public IHttpActionResult Activate(string hash,long UserID)
        {

            ActivateUserResponse success = ActivateUserResponse.RETURN_SUCCESS;
            //     ObjectParameter success = new ObjectParameter("success", typeof(int));
          
            try
            {
                long UID = UserID;
                returnValue rc = uc.ActivateUser(hash, UID, out success);

                if (rc == returnValue.RETURN_SUCCESS)
                {

                    if ((ActivateUserResponse)success != ActivateUserResponse.RETURN_SUCCESS)
                    {
                        switch ((ActivateUserResponse)success)
                        {
                            case ActivateUserResponse.RETURN_FAIL:
                                {
                                    return BadRequest("General Failure");
                                }
                            case ActivateUserResponse.RETURN_INVALIDPARAM:
                                {
                                    return BadRequest("Invalid parameters");
                                }
                            case ActivateUserResponse.RETURN_NOTAUTHORIZED:
                                {
                                    return BadRequest("User not authorized");
                                }
                            case ActivateUserResponse.RETURN_USERNOTEXIST:
                                {
                                    return BadRequest("User does not exist");
                                }
                            default: break;
                        }
                    }

                }
                else
                {
                    return BadRequest(uc.getError());
                }
            }
            catch (DataExpectedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NullReferenceException)
            {
                return BadRequest("NullReference");
            }
            catch (Exception)
            {
                return BadRequest("General Failure");
            }

            return Ok("");
        }



        [ResponseType(typeof(Users))]
        [HttpGet]
        public IHttpActionResult GetUsers(string hash,string search)
        {
            int count = 0;

            try
            { 
                List<Users> u = uc.GetUsers(hash,  search, out count);

                if (count == -1 )
                    return UserNotAuthorized();
                return Ok(u);
            }
            catch(DataExpectedException )
            {

            }
            catch(Exception )
            {

            }

            return Ok(new List<Users>());
        }

        [ResponseType(typeof(List<UserSearchResults>))]
        [HttpPost]
        public IHttpActionResult GetUserSearch(UserSearchRequest req)
        {
            // ObjectParameter count = new ObjectParameter("Count", typeof(int));
            int count = 0;
            Regex rgx = new Regex(@"\d{4}-\d{2}-\d{2}");
            Regex rgx2 = new Regex(@"\d{4}/\d{2}/\d{2}");

            if ( !ModelState.IsValid)
            {
                return BadRequest();
            }

            if ( !String.IsNullOrEmpty(req.SupplierCode))
            {
                if (req.SupplierCode.Length != 4)
                    return BadRequest();
            }

            if ( ! String.IsNullOrEmpty(req.DateFrom) )
            {
                   
                if (!rgx.IsMatch(req.DateFrom)  && !rgx2.IsMatch(req.DateFrom) )
                    req.DateFrom = null;
                if (! String.IsNullOrEmpty(req.DateFrom) &&  rgx2.IsMatch(req.DateFrom) )
                {
                    string tmp = req.DateFrom;
                    string dd = tmp.Substring(0, 2);
                    string mm = tmp.Substring(3, 2);
                    string yyyy = tmp.Substring(6, 4);
                    req.DateFrom = String.Format("{0}-{1}-{2}", yyyy, mm, dd);
                }
            }
            if ( !String.IsNullOrEmpty(req.DateFrom))
            {
                req.DateFrom = req.DateFrom.Substring(0,10) + " 00:00";
            }
            if (! String.IsNullOrEmpty(req.DateTo) )
            {
                if (!rgx.IsMatch(req.DateTo) && !rgx2.IsMatch(req.DateTo ) )
                    req.DateTo = null;

                if (!String.IsNullOrEmpty(req.DateTo) &&  rgx2.IsMatch(req.DateTo))
                {
                    string tmp = req.DateTo;
                    string dd = tmp.Substring(0, 2);
                    string mm = tmp.Substring(3, 2);
                    string yyyy = tmp.Substring(6, 4);
                    req.DateTo = String.Format("{0}-{1}-{2}", yyyy, mm, dd);
                }
            }
            if (!String.IsNullOrEmpty(req.DateTo))
            {
                req.DateTo = req.DateTo.Substring(0,10) + " 23:59";
            }
            List<AuditUserDetails> u = new List<AuditUserDetails>();
            try
            {
//                u = db.getAuditUserDetails(req.Hash, count, req.SupplierCode, req.User, req.DateFrom, req.DateTo).ToList();
                u = uc.GetAuditUserDetails(req.Hash, out count, req.SupplierCode, req.User, req.DateFrom, req.DateTo);
                if (u == null)
                {
                    return UserNotAuthorized();
                }
            } 
            catch(DataExpectedException )
            {
                u.Clear();
            }
            catch (Exception)
            {
                u.Clear();
            }
            List<UserSearchResults> results = new List<UserSearchResults>();
            foreach ( AuditUserDetails u1 in u)
            {
                UserSearchResults res = new UserSearchResults();
                res.Import(u1);
                results.Add(res);
            }

            return Ok(results);
        }


        [ResponseType(typeof(List<AdminLevels>))]
        [HttpGet]
        public IHttpActionResult GetCreateAdminLevels(string hash,long UserID)
        {
            
            try
            {
                List<AdminLevels> al = uc.GetCreateAdminLevels(hash, UserID);
                return Ok(al);
            }
            catch(DataExpectedException)
            {
                return BadRequest("Missing data in request");
            }
            catch (Exception)
            {
                List<AdminLevels> al2 = new List<AdminLevels>();
                return Ok (al2);
            }


            
        }

        [ResponseType(typeof(string))]
        private IHttpActionResult UserNotAuthorized()
        {
            return Ok (new InvalidUser("User not authorized"));
        }

        [ResponseType(typeof(string))]
        private IHttpActionResult setError(string msg)
        {
            return Ok(new InvalidUser(msg));
        }

        [ResponseType(typeof(string))]
        private IHttpActionResult UnknownError()
        {
            return Ok(new InvalidUser("Unknown error"));
        }

        [ResponseType(typeof(User))]
        [HttpGet]
        public IHttpActionResult GetUser(string hash, long UserID)
        {

            returnValue rc = returnValue.RETURN_SUCCESS;
            
            try
            {
                User u;
                rc = uc.GetUser(hash, UserID, out u);
                if (rc == returnValue.RETURN_SUCCESS)
                {
                    if (u == null || u.UserID == -1)
                    {
                        return UserNotAuthorized();
                    }
                    return Ok(u);
                }
                else
                {
                    return BadRequest(uc.getError());
                }
            }
            catch(DataExpectedException ex)
            {
                return (BadRequest(ex.Message));
            }
            catch(Exception ex)
            {
                return (BadRequest(ex.Message));
            }
        }
        [ResponseType(typeof(MyUserDetails))]
        [HttpGet]
        public IHttpActionResult GetMyDetails(string hash)
        {
//            ObjectParameter op = new ObjectParameter("ValidUser", typeof(int));
          

            returnValue rc;

            try
            {
                bool ValidUser = false;
                MyUserDetails u = new MyUserDetails();
                rc = uc.GetMyUserDetails(hash, out ValidUser ,  out u);
                if ( rc == returnValue.RETURN_SUCCESS)
                {
                    if ( u.UserID == 0)
                    {
                        return UserNotAuthorized();
                    }
                    return Ok(u);
                }
                else
                {
                    return BadRequest(uc.getError());
                }
            }
            catch (DataExpectedException ex)
            {
                return (BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                return (BadRequest(ex.Message));
            }


        }



        [ResponseType(typeof(AddUserResponse))]
        [HttpPost]
        public IHttpActionResult AddUser(AddUser AUser)
        {
            string ErrMsg = "";
            AddUserResponseEnum rsp = AddUserResponseEnum.RETURN_SUCCESS;
            returnValue rc = returnValue.RETURN_SUCCESS;

            uc.clearError();
            try
            {
                rc = uc.AddUser(AUser.Hash, AUser.UserName, AUser.SupplierCode, AUser.FullName, AUser.Telephone, AUser.AdminLevel, out rsp, out ErrMsg );
                if (rc == returnValue.RETURN_SUCCESS)
                {
                    if ( rsp != AddUserResponseEnum.RETURN_SUCCESS  )
                    {
                        return BadRequest(ErrMsg);
                    }


                }  
            }
            catch (DataExpectedException ex)
            {
                return (BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                return (BadRequest(ex.Message));
            }
/*
            returnValue rc2 = returnValue.RETURN_SUCCESS;
            try
            {
                uc.clearError();
                AddUserResponseEnum ur;
                rc2 = uc.AddUser(AUser.Hash, AUser.UserName, AUser.SupplierCode, AUser.FullName, AUser.Telephone, AUser.AdminLevel, out ur, out ErrMsg);
            }
            catch (DataExpectedException ex)
            {
                return (BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                return (BadRequest(ex.Message));
            }

//            db.addUser(AUser.Hash, AUser.UserName, AUser.SupplierCode, AUser.FullName, AUser.Telephone, AUser.AdminLevel, rc, ErrMsg);
*/
            AddUserResponse aur = new AddUserResponse();
            aur.ReturnCode = (int)rsp;
            aur.appendError(ErrMsg);
            List<AdminLevelRoles> al;
            string MVCRoleToUse = "";
            try
            {
                al = uc.GetAdminLevels(AUser.Hash);
                //                al = db.getAdminLevels(AUser.Hash).ToList<getAdminLevels>();

                if (al != null)
                {
                    int ix = al.FindIndex(a => a.AdminLevelNumber == AUser.AdminLevel);

                    if (ix != -1)
                    {
                        // found a matching admin level, extract mvcrole
                        if (!string.IsNullOrEmpty(al[ix].MVCRole))
                        {
                            MVCRoleToUse = al[ix].MVCRole.ToLower();
                        }
                    }
                }
            }
            catch (DataExpectedException ex)
            {
                aur.ReturnCode = 2;
                aur.appendError("Error in getAdminLevels () ");
                aur.appendError(ex.Message);

            }
            catch (Exception e)
            {
                aur.ReturnCode = 2;
                aur.appendError("Error in getAdminLevels () ");
                aur.appendError(e.Message);

            }


            RegisterBindingModel rbm = new RegisterBindingModel();


            rbm.AdminLevel = MVCRoleToUse;
            rbm.Email = AUser.UserName;
            //        rbm.Password = ConfigurationManager.AppSettings["defpass"];
            rbm.Password = rbm.Email;
            rbm.ConfirmPassword = rbm.Password;
            rbm.ApiKey = ConfigurationManager.AppSettings["ApiKey"];

            //          AccountController ac = new AccountController();
            //          await ac.Register(rbm);
            if (aur.ReturnCode == 0)
            {
                string environ = ConfigurationManager.AppSettings["environment"];

                string uri = ConfigurationManager.AppSettings[environ + "_apiaddress"] + "/api/Account/Register";
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(uri);


                    var responseTask = client.PostAsJsonAsync(client.BaseAddress, rbm);
                    responseTask.Wait();
                    var result = responseTask.Result;
                    aur.appendError(rbm.Email);
                    aur.appendError(String.Format("Status Code {0}", result.StatusCode));
                    if (result.IsSuccessStatusCode)
                    {
                    }
                }
            }





            return Ok(aur);
        }


        [ResponseType(typeof(UpdateUserResponse))]
        [HttpPut]
        public IHttpActionResult UpdateUser(UpdateUser uu)
        {
            //            ObjectParameter rc = new ObjectParameter("rc", typeof(int));
            //            ObjectParameter em = new ObjectParameter("errormessage", typeof(string));


            string ErrMsg = "";
            AddUserResponseEnum rsp = AddUserResponseEnum.RETURN_SUCCESS;
            returnValue rc = returnValue.RETURN_SUCCESS;

            uc.clearError();
            try
            {

                rc = uc.UpdateUser(uu.Hash, uu.UserID, uu.UserName, uu.FullName, uu.Telephone, uu.AdminLevel, out rsp, out ErrMsg);
                if (rc == returnValue.RETURN_SUCCESS)
                {
                    if (rsp == AddUserResponseEnum.RETURN_SUCCESS)
                    {
                        List<AdminLevelRoles> al = uc.GetAdminLevels(uu.Hash);
                        string MVCRoleToUse = "";
                        if (al != null)
                        {
                            int ix = al.FindIndex(a => a.AdminLevelNumber == uu.AdminLevel);

                            if (ix != -1)
                            {
                                // found a matching admin level, extract mvcrole
                                if (!string.IsNullOrEmpty(al[ix].MVCRole))
                                {
                                    MVCRoleToUse = al[ix].MVCRole.ToLower();
                                }
                            }
                        }

                        // update the Identity role for this user

                        IdentityRoleUpdateModel roleUpdate = new IdentityRoleUpdateModel();
                        roleUpdate.RoleName = MVCRoleToUse;
                        roleUpdate.ApiKey = ConfigurationManager.AppSettings["ApiKey"];
                        roleUpdate.IdentityEmail = uu.UserName;


                        string environ = ConfigurationManager.AppSettings["environment"];

                        string uri = ConfigurationManager.AppSettings[environ + "_apiaddress"] + "/api/Account/UpdateRole";
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(uri);

                            var responseTask = client.PostAsJsonAsync(client.BaseAddress, roleUpdate);
                            responseTask.Wait();
                            var result = responseTask.Result;
                            if (result.IsSuccessStatusCode)
                            {
                            }
                        }

                    }
                }



                UpdateUserResponse uur = new UpdateUserResponse();
                uur.ReturnCode = (int)rsp;
                if (!String.IsNullOrEmpty(ErrMsg))
                {
                    uur.appendError(ErrMsg);
                }

                return Ok(uur);
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


            
        }
    }
}
