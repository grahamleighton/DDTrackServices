using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDTRACK_DAL.Models.Common
{
    public enum DeActivateUserResponse
    {
        RETURN_SUCCESS = 0 ,
        RETURN_FAIL ,
        RETURN_NOTAUTHORIZED ,
        RETURN_USERNOTEXIST,
        RETURN_INVALIDPARAM
    };
    public enum ActivateUserResponse
    {
        RETURN_SUCCESS = 0,
        RETURN_FAIL,
        RETURN_NOTAUTHORIZED,
        RETURN_USERNOTEXIST,
        RETURN_INVALIDPARAM
    };
    public enum AddUserResponseEnum
    {
        RETURN_EXISTED = -1,
        RETURN_SUCCESS,
        RETURN_NOTAUTHORIZED,
        RETURN_FAILED,
        RETURN_INVALIDSUPPLIER
    };
    public class ReturnCodes
    {
    }
}