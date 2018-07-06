using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDTRACK_DAL.Models
{
    public class SupplierCode
    {
        public string Value { get; } 

        public bool isValid()
        {
            return ValidSupplier(Value);
        }
        public bool ValidSupplier(string SupplierCode)
        {
            if (SupplierCode.Length != 4)
                return false;

            if (SupplierCode == "0000") // special case
                return true;

            if (SupplierCode[0] >='A' && SupplierCode[0] <='Z' )
            {
                int i = 1;
                while ( i < 4 )
                {
                    if (SupplierCode[i] < '0' || SupplierCode[i] > '9')
                        return false;
                    i++;
                }
                return true;
            }

            return false;
        }

        public SupplierCode(string Supplier)
        {
            Value = "";
            if (ValidSupplier(Supplier))
                Value = Supplier;
        }
    }
}