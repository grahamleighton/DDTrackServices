using System;
using System.Web.Http;
using System.Web.Http.Description;
using DDTrackPlus.Controllers;
using DDTrackPlusCommon.Models;
using DDTRACK_DAL.Models.Common;

namespace DDTRACK_DAL.Controllers
{
    [Authorize]
    public class BarcodeController : ApiController
    {
        //     private DDTrackEntities db = new DDTrackEntities();
        private BarcodeDataController dc = new BarcodeDataController();

        [ResponseType(typeof(BarcodeInfo))]
        [HttpGet]
        public IHttpActionResult GetBarcodeInfo(long Id )
        {

            BarcodeInfo bi;

            try
            {
                bi = dc.GetBarcodeInfo(Id);
            }
            catch(DataExpectedException )
            {
                bi = new BarcodeInfo();
            }
            catch(NullReferenceException )
            {
                bi = new BarcodeInfo();

            }
            catch (Exception )
            {
                bi = new BarcodeInfo();
            }

            return Ok(bi);
        }

    }
}
