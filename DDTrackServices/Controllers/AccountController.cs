using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using DDTRACK_DAL.Models;

using Microsoft.Owin.Security.DataProtection;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Web.Http.Description;
using System.Data.Entity.Core.Objects;
using DDTrackPlus.Controllers;
using DDTrackServices;
using DDTrackServices.Results;
using DDTrackServices.Providers;

namespace DDTRACK_DAL.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;

  //      private DDTrackEntities db = new DDTrackEntities();
        private AccountDataController dc = new AccountDataController();


        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

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

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        private UserInfoViewModel GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            return new UserInfoViewModel
            {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };
        }

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (IdentityUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                UserManager.RemovePassword(User.Identity.GetUserId().ToString());
            }
            catch(Exception)
            {

            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);




//            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
//                model.NewPassword);
            
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        private async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        private async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        private async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                
                 ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        private IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }
        [AllowAnonymous]
        [Route("UpdateRole")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult UpdateRole(IdentityRoleUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.ApiKey != ConfigurationManager.AppSettings["ApiKey"])
            {
                ModelState.AddModelError("", "Invalid api key");
                return BadRequest(ModelState);
            }

            var user = UserManager.FindByEmail(model.IdentityEmail);

            if (user != null)
            {
                // found the user so update the role 
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));

                if (roleManager != null)
                {
                    // check if requested role exists , if not then add all roles
                    if (!roleManager.RoleExists(model.RoleName.ToLower()))
                    {
                        List<string> roles;
                        try
                        {
                            roles = dc.getMVCRoles();
                        }
                        catch(Exception ex)
                        {
                            roles = new List<string>();
                        }
//                        ObjectResult<string> roles = db.getMVCRoles();

                        foreach (string role in roles)
                        {
                            if (!roleManager.RoleExists(role))
                            {
                                var newrole = new IdentityRole();
                                newrole.Name = role.ToLower();
                                roleManager.Create(newrole);
                            }
                        }
                    }

                    // shouldn't occur but if doesn't exist then add it implicitly
                    if (!roleManager.RoleExists(model.RoleName.ToLower()))
                    {
                        var newrole = new IdentityRole();
                        newrole.Name = model.RoleName.ToLower();
                        roleManager.Create(newrole);
                    }
                    // now apply this role to the user

                    IList<string> currentroles = UserManager.GetRoles(user.Id);

                    if (currentroles != null)
                    {
                        foreach (string currentrole in currentroles)
                        {
                            UserManager.RemoveFromRole(user.Id, currentrole);
                        }
                    }
                    // now add the new role
                    UserManager.AddToRole(user.Id, model.RoleName.ToLower());
                    return Ok(true);

                }

            }
            else
            {
                return Ok(false);
            }
            return Ok(true);

        }


        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

           
            if (model.ApiKey != ConfigurationManager.AppSettings["ApiKey"] )
            {
                ModelState.AddModelError("","Invalid api key");
                return BadRequest(ModelState);
            }



            ApplicationDbContext context = new ApplicationDbContext();
            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            List<string> roles;
            try
            {
                roles = dc.getMVCRoles();
            }
            catch (Exception ex)
            {
                roles = new List<string>();
            }

//            ObjectResult<string>  roles = db.getMVCRoles();

            foreach( string role in roles )
            {
                if (!roleManager.RoleExists(role))
                {
                    var newrole = new IdentityRole();
                    newrole.Name = role.ToLower() ;
                    roleManager.Create(newrole);
                }
            }





            /*
             * if user is the admin account then set the password and don't require email confirmation
             * if user is a standard added one, allow the password to be set by the user with the email confirmation
             * mechanism
             */

            if (model.Email == ConfigurationManager.AppSettings["adminaccount"])
            {
                user.EmailConfirmed = true;
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }

                // add to admin role 
                if (roleManager.RoleExists("sysadmin"))
                {
                    UserManager.AddToRole(user.Id, "sysadmin");
                }

            }
            else
            {
                user.EmailConfirmed = false;
                IdentityResult result = await UserManager.CreateAsync(user, ConfigurationManager.AppSettings["defpass"]);

                bool RequireEmailConfirmation = true;
                try
                {
                    string emailconf = ConfigurationManager.AppSettings["NewUserRequiresConfirmationByEmail"];
                    if (!String.IsNullOrEmpty(emailconf))
                    {
                        char e = emailconf.Substring(1, 1).ToUpper()[0];
                        switch (e)
                        {
                            case 'Y':  //YES or No
                            case '1':  // 1 or 0
                            case 'T':  // true or false
                                {
                                    RequireEmailConfirmation = true;
                                    break;
                                }
                            default:
                                {
                                    RequireEmailConfirmation = false;
                                    break;
                                }
                        }
                        user.EmailConfirmed = !RequireEmailConfirmation;
                    }

                }
                catch(Exception)
                {
                    user.EmailConfirmed = false;
                }   

                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }

                if (roleManager.RoleExists(model.AdminLevel.ToLower()))
                {
                    UserManager.AddToRole(user.Id, model.AdminLevel.ToLower());
                }

                string callbackUrl = await SendEmailConfirmationTokenAsync(user.Id, "Confirm your account");

            }



            return Ok();
        }

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        private async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return InternalServerError();
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };
           
       
            IdentityResult result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result); 
            }
            return Ok();
        }


        private async Task<string> SendRecoveryConfirmationTokenAsync(string userID, string subject)
        {
            string code = await UserManager.GeneratePasswordResetTokenAsync(userID);
            string codeHtmlVersion = HttpUtility.UrlEncode(code);
            string environ = ConfigurationManager.AppSettings["environment"];
            string callbackUrl = ConfigurationManager.AppSettings[environ + "_mainaddress"] + "/Login/ConfirmRecovery?userId=" + userID + "&code=" + codeHtmlVersion;

            await SendMessage(userID, subject, "Please reset your passoword by clicking <a href=\"" + callbackUrl + "\">here</a>");

            return callbackUrl;
        }


        private async Task<string> SendEmailConfirmationTokenAsync(string userID, string subject)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);
            string codeHtmlVersion = HttpUtility.UrlEncode(code);
            string environ = ConfigurationManager.AppSettings["environment"];
            string callbackUrl = ConfigurationManager.AppSettings[environ + "_mainaddress"] + "/Login/ConfirmRegistration?userId=" + userID + "&code=" +    codeHtmlVersion;

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
        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        
        [HttpGet]
        private async Task<IHttpActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest();
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            string response2 = result.Succeeded ? "ConfirmEmail" : "Error";

            //            var response = Request.CreateResponse(HttpStatusCode.Moved);
            //            string suri = new Uri("http://localhost:57836/Home/ConfirmEmailResults").ToString();
            //            response.Headers.Location = new Uri(suri);
            //            return Ok(response);

            string environ = ConfigurationManager.AppSettings["environment"];

            string mainSite = ConfigurationManager.AppSettings[environ + "_mainaddress"] + "/Home/ConfirmEmailResults";  



            return Redirect(mainSite);
        }
        [AllowAnonymous]
        [HttpPost]
        [ResponseType(typeof(RegistrationResponse))]
        public async Task<IHttpActionResult> ConfirmRegistration(Registration reg)
        {
            RegistrationResponse RegistrationResponse = new RegistrationResponse();

            reg.Copy(RegistrationResponse);

            if ( ! reg.validateApiKey()  )
            {
                return Unauthorized();
            }
            if (reg.userid == null || reg.code == null)
            {
                return BadRequest();
            }
            var result = await UserManager.ConfirmEmailAsync(reg.userid, reg.code);
            string response2 = result.Succeeded ? "ConfirmEmail" : "Error";

            RegistrationResponse.Status = result.Succeeded;
            if (RegistrationResponse.Status == false)
                RegistrationResponse.Message = "Failed to register";
            else
                RegistrationResponse.Message = "Registration Successful";

            //            var response = Request.CreateResponse(HttpStatusCode.Moved);
            //            string suri = new Uri("http://localhost:57836/Home/ConfirmEmailResults").ToString();
            //            response.Headers.Location = new Uri(suri);
            //            return Ok(response);

            return Ok(RegistrationResponse);

        }

        [AllowAnonymous]
        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> ConfirmRecovery(Recovery reg)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                UserManager.RemovePassword(reg.userid);
            }
            catch (Exception)
            {

            }

            IdentityResult result = await UserManager.AddPasswordAsync(reg.userid, ConfigurationManager.AppSettings["defpass"]  );

            //            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
            //                model.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest();
            }

            return Ok();

        }


        private async Task<int> SendMessage(IdentityMessage message)
        {
            int resp = 0;
            var myMessage = new MailMessage();
            string destinationemail = message.Destination;

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["RegisterEmail"]))
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
                smtp.Port = Convert.ToInt32(ConfigurationManager.AppSettings["email_port"]);

                smtp.EnableSsl = false;
                Task t = smtp.SendMailAsync(myMessage);
                try
                {
                    await t;
                }
                catch (Exception ex)
                {
                    string xx = ex.Message;
                    throw new Exception(ex.Message);

                }

                resp = 1;
            }
            return resp;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
}
