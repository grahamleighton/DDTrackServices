using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Web;





namespace DDTRACK_DAL.Models.Common
{
    public enum returnValue { RETURN_SUCCESS = 0, RETURN_FAILURE , RETURN_INVALIDARGS , RETURN_DATAEXPECTED};
    public enum checkBehavior {  RETURN_ERROR = 0, RETURN_DEFAULT }
    public enum AdminLevelEnum { SYSTEM_ADMINISTRATOR = 0 , SUPPLIER_ADMINISTRATOR, USER , READONLY_USER ,DDADMIN , FTUSER }
    public class Common
    {
    }

  

    public class DataExpectedException : Exception
    {
        public DataExpectedException(string message) : base(message)
        {
        }
    }

    public class Checks
    {
       

        public Checks() { }

        public string GetDefaultFor(string arg, string argdefault = "" )
        {
            string retval = arg;
            if ( String.IsNullOrEmpty(arg) )
            {
                retval = argdefault;   
            }
            return retval;
        }

        public bool IsValid(string arg ,string ExceptionMessage )
        {
            if ( arg == null )
            {
                if ( ! String.IsNullOrEmpty(ExceptionMessage))
                {
                    throw new NullReferenceException(ExceptionMessage);
                }
            }
            return !String.IsNullOrEmpty(arg);
        }
        public bool HasData(string arg, string ExceptionMessage)
        {
            if (arg == null)
            {
                if (!String.IsNullOrEmpty(ExceptionMessage))
                {
                    throw new NullReferenceException(ExceptionMessage);
                }
            }
            if ( String.IsNullOrEmpty(arg))
            {
                if (!String.IsNullOrEmpty(ExceptionMessage))
                {
                    throw new DataExpectedException(ExceptionMessage);
                }
            }
            return !String.IsNullOrEmpty(arg);
        }
        public object GetDefaultForDB ( object val , object def)
        {
            if (val == DBNull.Value)
                return def;
            return val;
        }
        public int GetDefaultFor(int? arg, int argdefault = 0)
        {
            if (arg == null )
            {
                return argdefault;
            }
            else
            {
                return (int)arg;
            }
        }
        public long GetDefaultFor(long? arg, long argdefault = 0)
        {
            if (arg == null)
            {
                return argdefault;
            }
            else
            {
                return (long)arg;
            }
        }
        public Boolean GetDefaultFor(Boolean? arg, Boolean argdefault = false)
        {
            if (arg == null)
            {
                return argdefault;
            }
            else
            {
                return (Boolean)arg;
            }
        }
     
        public DateTime GetDefaultFor(DateTime? arg, string argdefault = "01/01/1980" )
        {
            if (arg == null)
            {
                try
                {
                    int year = Convert.ToInt32(argdefault.Substring(6, 4));
                    int month = Convert.ToInt32(argdefault.Substring(3, 2));
                    int day = Convert.ToInt32(argdefault.Substring(0, 2));

                    return new DateTime(year, month, day);
                }
                catch(Exception)
                {
                    return new DateTime(1980, 1, 1);
                }
            }
            else
            {
                return (DateTime)arg;
            }
        }

        public DateTime GetDefaultFor(DateTime? arg)
        {
            if (arg == null)
            {
                return new DateTime(1980,1,1);
            }
            else
            {
                return (DateTime)arg;
            }
        }

        public bool IsValid(int ?arg, string ExceptionMessage)
        {
            if (arg == null)
            {
                if (!String.IsNullOrEmpty(ExceptionMessage))
                {
                    throw new NullReferenceException(ExceptionMessage);
                }
            }
            return !(arg == null);
        }

        public bool HasData(int ?arg, string ExceptionMessage)
        {
            if (arg == null)
            {
                if (!String.IsNullOrEmpty(ExceptionMessage))
                {
                    throw new NullReferenceException(ExceptionMessage);
                }
            }
            if (arg == 0 )
            {
                if (!String.IsNullOrEmpty(ExceptionMessage))
                {
                    throw new DataExpectedException(ExceptionMessage);
                }
            }
            return ( arg != 0 ) ;
        }


        public bool IsValid(long? arg, string ExceptionMessage)
        {
            if (arg == null)
            {
                if (!String.IsNullOrEmpty(ExceptionMessage))
                {
                    throw new NullReferenceException(ExceptionMessage);
                }
            }
            return !(arg == null);
        }

        public bool HasData(long? arg, string ExceptionMessage)
        {
            if (arg == null)
            {
                if (!String.IsNullOrEmpty(ExceptionMessage))
                {
                    throw new NullReferenceException(ExceptionMessage);
                }
            }
            if (arg == 0)
            {
                if (!String.IsNullOrEmpty(ExceptionMessage))
                {
                    throw new DataExpectedException(ExceptionMessage);
                }
            }
            return (arg != 0);
        }

    }
}