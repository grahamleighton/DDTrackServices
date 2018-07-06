using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DDTRACK_DAL.Models
{

    public class RecoverLostPassword
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string ApiKey { get; set; }

        public bool validateKey(string key)
        {
            if (String.IsNullOrEmpty(key))
                return false;
            if (String.IsNullOrEmpty(ApiKey))
                return false;

            return (key == ApiKey);
        }
    }

    public class RecoverLostPasswordResponse : RecoverLostPassword
    {
        public bool Status { get; set; }
        public string Message { get; set; }

        public RecoverLostPasswordResponse()
        {
            Status = true;
            Message = "";
        }
        public RecoverLostPasswordResponse(RecoverLostPassword rlp)
        {
            this.ApiKey = rlp.ApiKey;
            this.Email = rlp.Email;
            Message = "";
            Status = true;
        }
    }
}