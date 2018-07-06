using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDTRACK_DAL.Models
{
    public class Hash
    {
        private string _value;
        public string Value { get { return _getValue();  }  } 

        private string _getValue()
        {
            return _value;
        }

        public Hash()
        {
            string h = new string(' ', 36);
            _value = h;
        }
        public Hash(string hash)
        {
            _value = hash;
        }
        public string getHash()
        {
            return _value;
        }

        public void setHash(string hash)
        {
            if ( validHash(hash))
                _value = hash;
        }
        public bool isValid()
        {
            return validHash(_value);
        }
        public bool validHash(string hash)
        {
            if (hash.Length == 0)
            {
                return true;
            }
            if (hash.Length == 1 && hash == "0")
            {
                return true;
            }
            if ( hash.Length == 36 )
            {
                List<string> tok = hash.Split('-').ToList();
                if ( tok.Count == 5 )
                {
                    return true;
                }
            }

            return false;
        }

    }
}