using DDTRACK_DAL.Models;
using DDTRACK_DAL.Models.Common;
using DDTrackPlus.Controllers;
using DDTrackServices;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Objects;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using static DDTrackPlus.Controllers.LoginDataController;

namespace DDTRACK_DAL.Controllers
{


   

    [Authorize]
    public class LoginController : ApiController
    {
        //        private DDTrackEntities db = new DDTrackEntities();   
        private LoginDataController dc = new LoginDataController();

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public async Task<bool> checkUserHasConfirmedEmail(string email)
        {
            bool Result = false;
            ApplicationUser user = await UserManager.FindByEmailAsync(email);

            if ( user != null )
            {
                Result = user.EmailConfirmed;
            }
            return Result;
        }

        private async Task<string> SendEmailConfirmationTokenAsync(string userID, string subject)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);
            string codeHtmlVersion = HttpUtility.UrlEncode(code);
            string environ = ConfigurationManager.AppSettings["environment"];

            string callbackUrl = ConfigurationManager.AppSettings[environ + "_mainaddress"] + "/Login/ConfirmRegistration?userId=" + userID + "&code=" + codeHtmlVersion;

            await SendMessage(userID, subject, "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

            return callbackUrl;
        }

        private async Task<int> SendMessage(string userID, string Subject, string Body)
        {
            int resp = 0;
            ApplicationUser user = UserManager.FindById(userID);
            if (user != null)
            {
                IdentityMessage im = new IdentityMessage();
                im.Subject = Subject;
                im.Body = Body;
                im.Destination = user.Email;
                resp = await SendMessage(im);

            }
            return resp;
        }
        private async Task<int> SendMessage(IdentityMessage message)
        {

            var myMessage = new MailMessage();

            string destinationemail = message.Destination;

            if ( !String.IsNullOrEmpty ( ConfigurationManager.AppSettings["RegisterEmail"]  ) )
            {
                destinationemail = ConfigurationManager.AppSettings["RegisterEmail"];

            }


            myMessage.To.Add(destinationemail);
            myMessage.Subject = message.Subject;

            myMessage.From = new MailAddress(ConfigurationManager.AppSettings["emailfrom"],
                    ConfigurationManager.AppSettings["emailfromdisplayname"]);


            myMessage.IsBodyHtml = true;
            myMessage.Body = message.Body;

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "ddtrack",  // replace with valid value
                    Password = ""  // replace with valid value
                };
                smtp.Credentials = credential;
                smtp.Host = ConfigurationManager.AppSettings["email_host"];
                smtp.Port = Convert.ToInt32(ConfigurationManager.AppSettings["email_port"]) ;
                smtp.EnableSsl = false;

                Task t = smtp.SendMailAsync(myMessage);
                await t;


                return 1;
            }
            //return 0;
        }

        private int CheckForSystemUser(LoginRequest req)
        {


            if (req.UserID == ConfigurationManager.AppSettings["adminaccount"] && req.Password == ConfigurationManager.AppSettings["adminpass"] )
            {
                dc.AddSimpleAdmin(req.UserID);
                ApplicationUser u;
                try
                {
                    // attempt to find the user to see if we exist and whether we have any "issues"

                    var UMResponse = UserManager.FindByEmailAsync(req.UserID);
                    UMResponse.Wait();
                    u = UMResponse.Result;
                    if ( u != null )
                    {
                        return 1;
                    }

                }
                catch (Exception)
                {
                    return 0;
                }
                // not currently a user so  
                // need to add to identity

                RegisterBindingModel rbm = new RegisterBindingModel();
                rbm.ApiKey = ConfigurationManager.AppSettings["ApiKey"];
                rbm.Email = ConfigurationManager.AppSettings["adminaccount"];
                rbm.Password = ConfigurationManager.AppSettings["adminpass"];
                rbm.AdminLevel = "sysadmin";
                rbm.ConfirmPassword = rbm.Password;

                bool RegisterSuccess = false;
                string environ = ConfigurationManager.AppSettings["environment"];

                string uri = ConfigurationManager.AppSettings[environ + "_apiaddress"] + "/api/Account/Register";
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(uri);

                    var responseTask = client.PostAsJsonAsync(client.BaseAddress, rbm);
                    responseTask.Wait();
                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        RegisterSuccess = true;
                       
                    }
                }
                if (RegisterSuccess)
                {
                    // ok , we have registered successfully , so lets retry the log in
                    return 1;
                }
            }

            return 0;

        }


        [ResponseType(typeof(LoginResponse))]
        [HttpPost]
        [AllowAnonymous]
       
        public async Task<IHttpActionResult> LoginUser(LoginRequest req)
        {

            // create and copy copy main details back to response message
            LoginResponse lr = new LoginResponse(req);
            bool NeedSQLLogIn = false;
            bool LogInSuccess = false;


            try
            {
                // check for system user and add if necessary
                int rc = CheckForSystemUser(req);
            }
            catch(Exception)
            {

            }


            
            string uri = "";
            string environ = ConfigurationManager.AppSettings["environment"];

            // get Token endpoint
            uri = ConfigurationManager.AppSettings[environ + "_apiaddress"] + "/Token";

            // attempt to find the user first 
            ApplicationUser u;
            try
            {
                // attempt to find the user to see if we exist and whether we have any "issues"

                var UMResponse = UserManager.FindByEmailAsync(req.UserID);
                UMResponse.Wait();
                u = UMResponse.Result;

            }
            catch(Exception ex)
            {
                lr.Status = -1;
       //         lr.RoleRequired = ex.Message;
                return Ok(lr);
            }
            
            if (u != null)
            {
                // we have found a user , lets see if it has been confirmed yet or not.

                if (u.EmailConfirmed == false)
                {
                    lr.EmailRequiresConfirmation = true;
                    string callbackUrl = await SendEmailConfirmationTokenAsync(u.Id, "Confirm your account");


                    return Ok(lr);
                }

                if (ConfigurationManager.AppSettings["defpass"] != req.Password)
                {

                    // we have found a user , lets try and sign in with passed details
                    // but only if the passed password is different to the default password
                    // NB : it WILL be the default password is user has left the password field blank on the 
                    // log in page

                    try
                    {
                        // attempt sign in with passed password
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(uri);
                            var content = new FormUrlEncodedContent(new[]
                            {
                        new KeyValuePair<string,string>("grant_type","password"),
                        new KeyValuePair<string,string>("username",req.UserID),
                        new KeyValuePair<string,string>("password",req.Password),
                    });

                            var responseTask = client.PostAsync(client.BaseAddress, content);
                            responseTask.Wait();
                            var result = responseTask.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                AccessToken at = new AccessToken();
                                var readTask = result.Content.ReadAsAsync<AccessToken>();
                                readTask.Wait();
                                at = readTask.Result;
                                lr.access_token = at.access_token;
                                NeedSQLLogIn = true;
                                
                                lr.RequiresPassword = false;
                                LogInSuccess = true;
                                // signed in OK
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }


                if (!LogInSuccess)
                {
                    // last log in attempt failed 
                    // we have found a user , lets check to see whether this is a first time sign in
                    // if so password will be ConfigurationManager.AppSettings["defpass"]

                    try
                    {
                        // attempt sign in with passed password
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(uri);
                            var content = new FormUrlEncodedContent(new[]
                            {
                        new KeyValuePair<string,string>("grant_type","password"),
                        new KeyValuePair<string,string>("username",req.UserID),
                        new KeyValuePair<string,string>("password",ConfigurationManager.AppSettings["defpass"]),
                    });

                            var responseTask = client.PostAsync(client.BaseAddress, content);
                            responseTask.Wait();
                            var result = responseTask.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                AccessToken at = new AccessToken();
                                var readTask = result.Content.ReadAsAsync<AccessToken>();
                                readTask.Wait();
                                at = readTask.Result;
                                lr.access_token = at.access_token;
                                NeedSQLLogIn = true;
                                // signed in OK using default password , signal to client that password needs changing
                                lr.RequiresPassword = true;
                                LogInSuccess = true;
                                // signed in OK
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }

                if ( LogInSuccess && ! String.IsNullOrEmpty(req.RoleRequired) )
                {
                    // need to check the user is a member of the role requested

                    if ( ! UserManager.IsInRole(u.Id ,req.RoleRequired))
                    {
                        LogInSuccess = false;
                        lr.access_token = "";
                        lr.Token = "0";
                        lr.Change = "N";
                        NeedSQLLogIn = false;
                    }
                    
                }

                if (NeedSQLLogIn)
                {

                    try
                    {
                        /*                        ObjectParameter token = new ObjectParameter("token", typeof(string));
                                                ObjectParameter change = new ObjectParameter("change", typeof(string));

                        */
                        string token = new string(' ', 36);
                        string change = "";
                        if (!ModelState.IsValid)
                        {
                            return BadRequest();
                        }
                        lr.CopyRequest(req);
                        try
                        {
                            returnValue rc = dc.loginUser(req.UserID,  req.Supplier, out token, req.IPAddress, out change);
                            if ( rc != returnValue.RETURN_SUCCESS )
                            {
                                lr.Token = dc.getError();
                            }
                            else
                            {
                                lr.Token = (string)token;
                                lr.Change = (string)change;
                            //    lr.Password = "";
                            }
                        }
                        catch (Exception e)
                        {
                   //         lr.IPAddress = e.Message;
                        }


                  //      lr.RequiresPassword = false;



                    }
                    catch (Exception e2)
                    {
                        if (lr != null)
                        {
                            lr.Token = e2.Message;
                        }
                    }
                }
            }
            return Ok(lr);
        }

      
        [ResponseType(typeof(string))]
        public IHttpActionResult CloneUser ( CloneRequest req)
        {
            string token = "0";
            if ( ! ModelState.IsValid )
            {
                return Ok(token);
            }

            string CloneToken = new string(' ', 36);

            returnValue rc = dc.cloneUser(req, out CloneToken);
            try
            {
                token = (string)CloneToken;
            }
            catch(Exception )
            {
                token = "0";
            }

            return (Ok(token));
        }


    }
}
