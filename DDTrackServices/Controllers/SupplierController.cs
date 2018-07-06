using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DDTRACK_DAL.Models;
using System.Web.Http.Description;
using System.Data.Entity.Core.Objects;
using DDTrackPlus.Controllers;
using DDTRACK_DAL.Models.Common;

namespace DDTRACK_DAL.Controllers
{
    [Authorize]
    public class SupplierController : ApiController
    {
   //     private DDTrackEntities db = new DDTrackEntities();
        private SupplierDataController dc = new SupplierDataController();

        [ResponseType(typeof(UserSupplierList))]
        [HttpGet]
        public IHttpActionResult GetUserSuppliers(string hash , long UserID )
        {

            List<UserSuppliers> us;

            try
            {
                us = dc.GetUserSuppliers(hash, (long)UserID).ToList();
            }
            catch(DataExpectedException )
            {
                us = new List<UserSuppliers>();
                us.Clear();
            }
            catch(NullReferenceException )
            {
                us = new List<UserSuppliers>();
                us.Clear();
            }
            catch (Exception )
            {
                us = new List<UserSuppliers>();
                us.Clear();
            }


            List<UserSupplierList> usl  = new List<UserSupplierList>();

            foreach ( UserSuppliers u in us )
            {
                UserSupplierList ul = new UserSupplierList();
                ul.Copy(u);
                usl.Add(ul);

            }
            return Ok(usl);
        }

        [ResponseType(typeof(DeleteUserSupplierResponse))]
        [HttpPost]
        public IHttpActionResult DeleteUserSupplier(DeleteUserSupplier dus)
        {
            int success = 0;
            returnValue rc = dc.DeleteUserSupplier(dus.Hash, dus.UserID, dus.SupplierID, out success);
//            db.deleteUserSupplier(dus.Hash, dus.UserID, dus.SupplierID,success );
            DeleteUserSupplierResponse dusr = new DeleteUserSupplierResponse();
            dusr.ReturnCode = -4;  // assume error to begin with
            if (rc == returnValue.RETURN_SUCCESS)
            {
                dusr.ReturnCode = success;

                switch (dusr.ReturnCode)
                {
                    case 0: dusr.appendError("Success"); break;
                    case -1: dusr.appendError("Not authorized"); break;
                    case -2: dusr.appendError("Invalid User"); break;
                    case -3: dusr.appendError("No match for supplier"); break;
                    case -4: dusr.appendError("General error occurred"); break;
                    default: break;
                }
            }

            return Ok(dusr);
        }

        [ResponseType(typeof(PostUserSupplierResponse))]
        [HttpPost]
        public IHttpActionResult PostUserSupplier(PostSupplier ps)
        {
            if ( ! ModelState.IsValid)
            {
                return BadRequest();
            }

//            ObjectParameter success = new ObjectParameter("success",typeof(int));
            int success = 0;

            returnValue rc = dc.AddUserSupplier(ps.Hash, ps.UserID, ps.SupplierID, ps.SupplierCode, out success);
            PostUserSupplierResponse pusr = new PostUserSupplierResponse();
            if (rc == returnValue.RETURN_SUCCESS)
            {
                //          db.addUserSupplier(ps.Hash, ps.UserID, ps.SupplierID, ps.SupplierCode, success);
                pusr.ReturnCode = success;
            }
            else
            {
                pusr.ReturnCode = 4;
            }

            switch (pusr.ReturnCode)
            {
                case 0: pusr.appendError("Invalid parameters"); break;
                case 1: pusr.appendError("Success"); break;
                case 2: pusr.appendError("Success , already added"); break;
                case 3: pusr.appendError("not authorized"); break;
                case 4: pusr.appendError("call failed"); break;
                case -1: pusr.appendError("no such supplier"); break;
                default: break;
            }

            return Ok(pusr);

        }




    }
}
