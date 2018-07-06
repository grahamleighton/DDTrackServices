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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace DDTRACK_DAL.Controllers
{
    [Authorize]
    public class LoginSecController : ApiController
    {
   //     private DDTrackEntities db = new DDTrackEntities();
        private LoginSecDataController dc = new LoginSecDataController();
        private UserDataController uc = new UserDataController();
        private Checks CodeGuard = new Checks();
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

        [AllowAnonymous]
        [HttpPost]
        [ResponseType(typeof(RecoverLostPasswordResponse))]
        public async Task<IHttpActionResult> RecoverLostPassword(RecoverLostPassword rlp)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            if ( ! rlp.validateKey(ConfigurationManager.AppSettings["ApiKey"] ) )
            {
                return BadRequest();
            }

            var userId = UserManager.FindByEmail(rlp.Email);

            var token = await UserManager.GeneratePasswordResetTokenAsync(userId.Id);

            RecoverLostPasswordResponse rlpr = new RecoverLostPasswordResponse(rlp);
            try
            {
                string callbackUrl = await SendRecoveryConfirmationTokenAsync(userId.Id, "Password Reset for DDTrack Plus");
            }
            catch(Exception ex)
            {
                rlpr.Status = true;
                rlpr.Message = ex.Message;

            }

            rlpr.Status = true;
            rlpr.Message = "Email has been sent to registered address. Please check your inbox";

            
            return Ok(rlpr);

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

        private async Task<string> SendRecoveryConfirmationTokenAsync(string userID, string subject)
        {
            string code = await UserManager.GeneratePasswordResetTokenAsync(userID);
            string codeHtmlVersion = HttpUtility.UrlEncode(code);
            string environ = ConfigurationManager.AppSettings["environment"];

            string callbackUrl = ConfigurationManager.AppSettings[environ + "_mainaddress"] + "/Login/Password?userId=" + userID + "&code=" + codeHtmlVersion;

            await SendMessage(userID, subject, "Please change your password by clicking <a href=\"" + callbackUrl + "\">here</a>");

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

                await smtp.SendMailAsync(myMessage);
                return 1;
            }
           // return 0;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IHttpActionResult> SetPassword2(SetPasswordBindingModel sp)
        {

            var user = await UserManager.FindByIdAsync(sp.userid);

            if (user == null)
                return BadRequest();


            var result = await UserManager.ResetPasswordAsync(user.Id, sp.code, sp.ConfirmPassword);
            


            if ( result.Succeeded)
            {
                ObjectParameter op = new ObjectParameter("success", typeof(int));

        //        db.setPasswordChange(sp.Hash, sp.UserID, op);


                return Ok();

            }

            return BadRequest();

        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IHttpActionResult> SetPassword(UserClasses sp)
        {
            //            ObjectParameter op = new ObjectParameter("success", typeof(int));
            //            db.setPassword(sp.Hash, sp.UserID, sp.Password, op);

            //            var userId = User.Identity.GetUserId();
            User u = new User();
            returnValue rc = uc.GetUser(sp.Hash, sp.UserID, out u);
            if (String.IsNullOrEmpty(u.UserName) || rc == returnValue.RETURN_FAILURE)
                return BadRequest();

            var userId = UserManager.FindByEmail(u.UserName);

            if (userId == null)
                return BadRequest();

            var token = await UserManager.GeneratePasswordResetTokenAsync(userId.Id);

            var result = await UserManager.ResetPasswordAsync(userId.Id, token, sp.Password);

            if (result.Succeeded)
            {
              //  ObjectParameter op = new ObjectParameter("success", typeof(int));
                rc = dc.setPasswordChange(token, sp.UserID);
                
                if (rc == returnValue.RETURN_SUCCESS)
                    return Ok();
//                db.setPasswordChange(sp.Hash, sp.UserID, op);
                
            }
            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost]
       
        public async Task<IHttpActionResult> SetPasswordForUser(PasswordChange pw)
        {
            //            ObjectParameter op = new ObjectParameter("success", typeof(int));
            //            db.setPassword(sp.Hash, sp.UserID, sp.Password, op);

            //            var userId = User.Identity.GetUserId();
//            User u = new User();
//            returnValue rc = uc.GetUser(sp.Hash, sp.UserID, out u);
//            if (String.IsNullOrEmpty(u.UserName) || rc == returnValue.RETURN_FAILURE)
//                return BadRequest();

            var userId = UserManager.FindByEmail(pw.UserName);

            if (userId == null)
                return BadRequest();

            var token = await UserManager.GeneratePasswordResetTokenAsync(userId.Id);

            var result = await UserManager.ResetPasswordAsync(userId.Id, token, pw.Password);

            if (result.Succeeded)
            {
                //  ObjectParameter op = new ObjectParameter("success", typeof(int));
                returnValue rc = dc.setPasswordForUser(pw.UserName);

                if (rc == returnValue.RETURN_SUCCESS)
                    return Ok();
                //                db.setPasswordChange(sp.Hash, sp.UserID, op);

            }
            return Unauthorized();
        }



        [ResponseType(typeof(bool))]
        [HttpPut]
        public IHttpActionResult SetPasswordAndSecurityQuestion(SetPasswordAndSecurity sp)
        {
//            ObjectParameter op = new ObjectParameter("success", typeof(int));

            returnValue rc = returnValue.RETURN_SUCCESS;
            rc = dc.setPasswordandSecurityQuestion(sp.Hash, sp.UserID, sp.Password, sp.SecurityQuestionID, sp.SecurityAnswer);
//            db.setPasswordandSecurityQuestion(sp.Hash, sp.UserID, sp.Password, sp.SecurityQuestionID, sp.SecurityAnswer, op);
            if ( rc != returnValue.RETURN_SUCCESS )
            {
                return BadRequest();
            }
            if ( dc.Error()  )
            {
                return BadRequest();
            }
            // update identity password
/*
            SetPasswordBindingModel pbm = new SetPasswordBindingModel();
            pbm.NewPassword = sp.Password;
            pbm.ConfirmPassword = pbm.NewPassword;
*/
        
            return Ok(true);
        }

        [ResponseType(typeof(SecurityQuestionResponse))]
        [HttpGet]
        public IHttpActionResult GetSecurityQuestion(string Hash, long UserID)
        {
            UserAccess ua = new UserAccess();
            ua.UserID = UserID;
            ua.Hash = Hash;

            SecurityQuestionResponse sqr = new SecurityQuestionResponse();
//            ObjectParameter question = new ObjectParameter("question", typeof(string));
            string question = "";
//            db.getSecurityQuestion( ua.UserID, question);
            returnValue rc = dc.getSecurityQuestion(ua.UserID, out question);

            question = CodeGuard.GetDefaultFor(question, "");

            if (String.IsNullOrEmpty(question) ) 
            {
                sqr.appendError("No question found");
            }
            sqr.SecurityQuestion = question;
            sqr.ReturnCode = sqr.ErrorMessage.Count;

            return Ok(sqr);
        }

        [ResponseType(typeof(ValidateRecoveryResponse))]
        [HttpGet]
        public IHttpActionResult ValidateRecovery(string Hash)
        {
//            ObjectParameter userid = new ObjectParameter("userid", typeof(long));
//            ObjectParameter valid = new ObjectParameter("valid", typeof(int));
//            ObjectParameter suppliercode = new ObjectParameter("suppliercode", typeof(string));
            ValidateRecoveryResponse vcr = new ValidateRecoveryResponse();

            long userid = 0;
            int valid = 0;
            string suppliercode = "";


            returnValue rc = dc.validateRecovery(Hash, out userid , out valid, out suppliercode);
//            db.validateRecovery(Hash, userid, valid, suppliercode);
            if (valid == 1)
            {
                vcr.SupplierCode = suppliercode;
                vcr.UserID = userid;
                vcr.Valid = valid;
            }
             
            return Ok(vcr);
        }

        [ResponseType(typeof(bool))]
        [HttpGet]
        public IHttpActionResult IsEmailAvailable(string Hash , string UserName )
        {
            // ObjectParameter exists = new ObjectParameter("exists", typeof(int));
            bool exists = false;

            returnValue rc = dc.isEmailAvailable(Hash, UserName, out exists);
//            db.isEmailAvailable(Hash, UserName, exists);
            
            if (rc == returnValue.RETURN_SUCCESS &&  exists)
            {
                return Ok(true);
            }

            return Ok(false);
        }
        [ResponseType(typeof(bool))]
        [HttpGet]
        public IHttpActionResult IsSupplierAvailable(string Hash , string SupplierCode )
        {
//            ObjectParameter exists = new ObjectParameter("exists", typeof(int));
            bool exists = false;

            returnValue rc = dc.isSupplierAvailable(Hash, SupplierCode, out exists);

//            db.isSupplierAvailable(Hash, SupplierCode, exists);
            if ( rc == returnValue.RETURN_SUCCESS && exists)
            {
                return Ok(false);
            }

            return Ok(true);
        }

        [ResponseType(typeof(bool))]
        [HttpPost]
        public IHttpActionResult CheckSecurityAnswer(CheckSecurity cs)
        {
//            ObjectParameter correct = new ObjectParameter("correct", typeof(int));
            int correct = 0;
           


            returnValue rc =  dc.checkSecurityAnswer(cs.Hash, cs.UserID, cs.Answer, out correct);
//            db.checkSecurityAnswer(cs.Hash, cs.UserID, cs.Answer, correct);

            if ( rc == returnValue.RETURN_SUCCESS && correct == 1)
            {
                return Ok(true);
            }

            return Ok(false);
        }

        [ResponseType(typeof(SecurityQuestions))]
        [HttpGet]
        public IHttpActionResult GetSecurityQuestions( string hash )
        {
            if (hash == null)
                hash = "xxx";
            List<SecurityQuestions> sq = new List<SecurityQuestions>();

            returnValue rc = dc.getSecurityQuestions(hash, out sq);
            if ( rc == returnValue.RETURN_SUCCESS)
            {
                return Ok(sq.ToList());
            }
            sq.Clear();
            return Ok(sq.ToList());
        }

        [ResponseType(typeof(RecoveryUserResponse))]
        [HttpPost]
        public IHttpActionResult GetRecoveryUser(RecoveryUser ru)
        {
//            ObjectParameter userid = new ObjectParameter("userid", typeof(long));

            long userid;
            RecoveryUserResponse rur = new RecoveryUserResponse();

            returnValue rc = dc.getRecoveryUser(ru.UserName, ru.SupplierCode, out userid);
            //            db.getRecoveryUser(ru.UserName, ru.SupplierCode, userid);
            if (rc == returnValue.RETURN_SUCCESS)
            {
                rur.UserID = userid;
            }

            return Ok(rur);
        }

        [ResponseType(typeof(RecoveryEmailResponse))]
        [HttpGet]
        public IHttpActionResult GetRecoveryEmail(long id)
        {
            /*
                        ObjectParameter body = new ObjectParameter("body", typeof(string));
                        ObjectParameter subject = new ObjectParameter("subject", typeof(string));
                        ObjectParameter emailaddress = new ObjectParameter("emailaddress", typeof(string));
            */
            string body = "";
            string subject = "";
            string emailaddress = "";

            returnValue rc =  dc.getRecoveryEmail(id, out body, out subject, out emailaddress);
//            db.getRecoveryEmail(id, body, subject, emailaddress);
            RecoveryEmailResponse rer = new RecoveryEmailResponse();
            if ( rc == returnValue.RETURN_SUCCESS && !String.IsNullOrEmpty(body) )
            {
                rer.EmailAddress = emailaddress;
                rer.Subject = subject;
                rer.Body = body;
            }

            return Ok(rer);
        }

    }
}
