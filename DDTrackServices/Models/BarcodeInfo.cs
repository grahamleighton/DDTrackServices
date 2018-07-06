using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDTrackPlusCommon.Models
{
    public class BarcodeInfo
    {
        public string Barcode { get; set; }
        public string VanRound { get; set; }

        public BarcodeInfo()
        {
            Clear();
        }
        public void Clear()
        {
            Barcode = "";
            VanRound = "";
        }
    }
}