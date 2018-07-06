using System;
using System.Data.SqlClient;
using System.Data;

using DDTrackPlusCommon.Models;
using DDTRACK_DAL.Models.Common;

namespace DDTrackPlus.Controllers
{
    public class BarcodeDataController : CommonDataController
    {
      
        //TODO:Rename to SupplierUser
        private Checks CodeGuard = new Checks();

        public BarcodeInfo GetBarcodeInfo(long Id)
        {

            CodeGuard.HasData(Id, "Id is missing");
               clearError();

            BarcodeInfo bi = new BarcodeInfo();


            try
            {
                SqlCommand com = sql.sqlGetCommand("extapi.usp_GetBarcode");
                com.Parameters.AddWithValue("@Id",Id);

                if (!sql.sqlFillDataTable(com))
                    throw new Exception(sql.sqlErrorMessage());

                foreach (DataRow dr in sql.sqlDataTable().Rows)
                {

                    bi.Barcode = Convert.ToString(dr["FGHBarcode"]);
                    bi.VanRound = Convert.ToString(dr["VanRound"]);
                }
            }
            catch (Exception ex)
            {
                setError(ex.Message);
                bi.Clear();
                                
                throw new Exception(ex.Message);
            }

            return bi;

        }

      

     

    }
}