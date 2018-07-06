using DDTRACK_DAL.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DDTRACK_DAL.Models
{
    public class PasswordChange
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
  
    public class BaseAccess
    {
        [Required]
        public string Hash { get; set; }
        public string _display()
        {
            return ( "\" Hash : \"" + Hash + "\"" );
        }
    }
    public class UserAccess : BaseAccess
    { 
        public long UserID { get; set; }
        public new string _display()
        {
            return base._display() + ", UserID : \"" + Convert.ToString(UserID) + "\"";
        }
    }
    public class UserClasses : UserAccess
    {
        public string Password { get; set; }
        public new string _display()
        {
            return base._display() + ", Password : \"" + Password + "\"";
        }
    }

    public class CheckSecurity : UserAccess
    {
        public string Answer { get; set; }
        public new string _display()
        {
            return base._display() + ", Answer : \"" + Answer + "\"";
        }

    }

    public class SetPasswordAndSecurity : UserClasses
    {
        public long SecurityQuestionID { get; set; }
        public string SecurityAnswer { get; set; }
        public new string _display()
        {
            return base._display() + ", SecurityQuestionID : \"" + Convert.ToString(SecurityQuestionID) + "\" , SecurityAnswer : \"" + SecurityAnswer + "\"";
        }


    }

    public class RecoveryUser : UserAccess
    {
        public string UserName { get; set;  }
        public string SupplierCode { get; set; }
        public new string _display()
        {
            return base._display() + ", UserName: \"" + UserName + "\" , SupplierCode : \"" + SupplierCode + "\"";
        }

    }
    public class RecoveryUserResponse 
    {
        public long UserID { get; set;  }
        public string _display()
        {
            return "UserID : \"" + Convert.ToString(UserID) + "\"";
        }

    }
    public class RecoveryEmailResponse 
    {
        public string Body { get; set; }
      
        public string Subject { get; set; }
        public string EmailAddress { get; set; }
    }

    public class CloneRequest
    {
        [Required]
        [StringLength(36,MinimumLength =36)]
        public string Hash { get; set; }
        [Required]
        [Range(1, long.MaxValue)]        
        public long CloneUserID { get; set; }
    }
    public class LoginRequest
    {
        [Required]
        [MaxLength(250)]
        [RegularExpression(@"^[^\s\,]*$")]
        public string UserID { get; set; }
        public string Password { get; set; }
        [RegularExpression(@"^[^\s\,]*$")]
        public string Supplier { get; set; }
        public string IPAddress { get; set; }
        public string _display ()
        {
            return " UserID : \"" + UserID + "\", Password : \"" + Password + "\", Supplier : \"" + Supplier + "\", IPAddress : \"" + IPAddress + "\" ";
        }
        public string RoleRequired { get; set; }
      
    }

    public class UserSearchRequest
    {
        [Required]
        [StringLength(36,MinimumLength =36)]
        [RegularExpression(@"^\s*\S*\s*$")]

        public string Hash { get; set; }
        
        public string SupplierCode { get; set; }
        [StringLength(250)]
        [RegularExpression(@"^\s*\S*\s*$")]
        public string User { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
    }

    public class UserSearchResults
    {
        public string AuditTimeStamp { get; set; }
        public string EventDescription { get; set; }
        public string By { get; set; }
        public string For { get; set; }
        public string ForSupplierCode { get; set; }
        public string ForSupplierName { get; set; }
        public string BySupplierCode { get; set; }
        public string BySupplierName { get; set; }
        public long AuditID { get; set; }
        public void Import(AuditUserDetails a)
        {

            this.AuditTimeStamp = Convert.ToDateTime(a.AuditTimeStamp).ToString("yyyy/MM/dd HH:mm");
            
            this.By = a.By;
          
            this.EventDescription = a.EventDescription;
            this.For = a.For;
            this.ForSupplierCode = a.ForSupplierCode;
            this.ForSupplierName = a.ForSupplierName;
            this.AuditID = a.AuditID;

        }
    }

    public class ConnectTest
    {
        public string Service { get; set; }
        public string Data { get; set; }
        public string Env { get; set;  }
    }
    public class LoginResponse 
    {
        public string UserID { get; set; }
        public string Supplier { get; set; }

        public string Token { get; set; }
        public string Change { get; set; }
       
        public string access_token { get; set; }
        public Boolean EmailRequiresConfirmation { get; set; }
        public Boolean RequiresPassword { get; set; }
        public int Status { get; set;  }

        public void CopyRequest(LoginRequest r)
        {
            this.UserID = r.UserID;
            //                   this.Password = r.Password;
            this.Supplier = r.Supplier;
            //                   this.IPAddress = r.IPAddress;
        }

        public string _display()
        {
            return " UserID : \"" + UserID + "\", Supplier : \"" + Supplier + "\", Token : \"" + Token + "\", Change : \"" + Change + "\"";

        }
        public LoginResponse()
        {
            Token = "0";
            Change = "N";
            access_token = "";
            EmailRequiresConfirmation = false;
            RequiresPassword = false;
            Status = 0;
        }
        public LoginResponse(LoginRequest req)
        {
            Token = "0";
            Change = "N";
            access_token = "";
            EmailRequiresConfirmation = false;
            RequiresPassword = false;
            CopyRequest(req);
            Status = 0;
        }


    }
    public class ErrorMessages
    {
        public List<string> ErrorMessage;
        public ErrorMessages()
        {
            ErrorMessage = new List<string>();
        }
        public void appendError(string msg)
        {
            ErrorMessage.Add(msg);
        }
        public List<string> getMessages()
        {
            return ErrorMessage;
        }
        public string _display()
        {
            string disp = "";
            foreach ( string msg in ErrorMessage)
            {
                if ( String.IsNullOrEmpty(disp) )
                    disp = "ErrorMessage : \"" + msg + "\"";
                else
                    disp = disp + ", ErrorMessage : \"" + msg + "\"";
            }
            return disp;
        }
    }

    public class InvalidUser : ErrorMessages
    {

        public InvalidUser(string msg)
        {
            appendError(msg);
        }
    }
    public class SecurityQuestionResponse : ErrorMessages
    {
        public string SecurityQuestion { get; set; }
        public int ReturnCode { get; set; }
    }
    public class UpdateUser : UserAccess
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Telephone { get; set; }
        public int AdminLevel { get; set; }
        public new string _display()
        {
            return base._display() + ",UserName : \"" + UserName + "\" , FullName : \"" + FullName + "\", Telephone : \"" + Telephone + "\", AdminLevel : \"" + Convert.ToString(AdminLevel) + "\"" ;
        }

    }
    public class AddUser : UpdateUser
    {
       
        [MaxLength(4)]
        public string SupplierCode { get; set; }
        public new string _display()
        {
            return base._display() + ",SupplierCode : \"" + SupplierCode + "\""; 
        }

    }

  
    public class AddUserResponse : ErrorMessages
    {
        public int ReturnCode { get; set; }
        public new string _display()
        {
            return " Return Code : \"" + Convert.ToString(ReturnCode) + "\" , " + base._display() ;
        }
    }

    public class ValidateRecoveryResponse
    {
        public long UserID { get; set; }
        public int Valid { get; set; }
        public string SupplierCode { get; set; }
        public ValidateRecoveryResponse()
        {
            UserID = 0;
            Valid = 0;
            SupplierCode = "";
        }
    }

  

    public class UpdateUserResponse : ErrorMessages
    {
        public int ReturnCode { get; set;  }
    }

    public class EmailAvailable : BaseAccess
    {
        public string UserName { get; set;  } 
    }
    public class SupplierAvailable : BaseAccess
    {
        public string SupplierCode { get; set; }
    }

    public class PostSupplier : UserAccess
    {
        public long SupplierID { get; set;  }
        public string SupplierCode { get; set;  }
    }
    public class DeleteUserSupplier : UserAccess
    {
        public long SupplierID { get; set; }
    }
    public class DeleteUserSupplierResponse : ErrorMessages
    {
        public int ReturnCode { get; set;  }
    }
    public class PostUserSupplierResponse : ErrorMessages
    {
        public int ReturnCode { get; set; }
    }
    public class UserSupplierList
    {
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public long SupplierID { get; set; }
        public long UserSupplierID { get; set; }
        public void Copy(UserSuppliers us)
        {
            this.SupplierCode = us.SupplierCode;
            this.SupplierName = us.SupplierName;
            this.SupplierID = (long)us.SupplierID;
            this.UserSupplierID = (long)us.UserSupplierID;
        }
    }
    public class AdminLevels
    {
        public int AdminNumber { get; set; }
        public string AdminName { get; set;  }
        public void Copy(getAdminLevelsCreate_Result par)
        {
            this.AdminName = par.AdminLevelName;
            this.AdminNumber = (int)par.AdminLevelNumber;
        }
    }
    public class AdminLevelRoles 
    {
        public int AdminLevelNumber { get; set; }
        public string AdminLevelName { get; set; }
        public string MVCRole { get; set; }
    }

    public class AccessToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public long expires_in { get; set; }
        public string username { get; set; }
        public DateTime issued { get; set; }
        public DateTime expires { get; set; }
    }

    public class Registration
    {
        public string userid { get; set; }
        public string code { get; set; }
        public string ApiKey { get; set; }
        public bool validateApiKey ()
        {
            if (String.IsNullOrEmpty(ApiKey))
                return false;

            return (ApiKey == ConfigurationManager.AppSettings["ApiKey"]); 
        }
        public void Copy(RegistrationResponse reg)
        {
            reg.code = this.code;
            reg.userid = this.userid;

        }
    }

    public class Recovery
    {
        public string userid { get; set; }
        public string code { get; set; }
    }
    public class RegistrationResponse
    {
        public string userid { get; set; }
        public string code { get; set; }

        public bool Status { get; set; }
        public string Message { get; set; }

      
    }

    public class IdentityRoleUpdateModel
    {
        [Required]
        public string IdentityEmail { get; set; }
        [Required]
        public string ApiKey { get; set; }
        [Required]
        public string RoleName { get; set; } 
    }
}