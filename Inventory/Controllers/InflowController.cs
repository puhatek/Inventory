using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Inventory.Models;
using Inventory.Models.Inflows;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using Inventory.Models.Helpers;

namespace Inventory.Controllers
{
    public class InflowController : Controller
    {
        //public static Dictionary<int, List<InflowView>> ordersForInflow = new Dictionary<int, List<InflowView>>();
        public static Dictionary<int, List<InflowItem>> ordersForInflow = new Dictionary<int, List<InflowItem>>();
        
        Inflow inflow = new Inflow();
        InflowItem inflowItem = new InflowItem();

        MagazynDbContext db = new MagazynDbContext();

        //just to keep in mind that this is cool way of building statuses
        static int statusID = 1; //let's say it is a status linked with orders 
        static int ORDER_STATUT_AWAITING = statusID * 100 + 1,
                   ORDER_STATUT_PENDING = 1 * 100 + 2,
                   ORDER_STATUT_SENT = 1 * 100 + 3,
                   ORDER_STATUT_COMPLETED = 1 * 100 + 4;

        static string STATUS_AWAITING = Status.GetDbStatusName(StatusEn.awaiting),
                      STATUS_PENDING = Status.GetDbStatusName(StatusEn.pending),
                      STATUS_SENT = Status.GetDbStatusName(StatusEn.sent),
                      STATUS_COMPLETED = Status.GetDbStatusName(StatusEn.completed);

        // GET: Inflow
        public ActionResult Index()
        {
            InflowView model = new InflowView();
            ordersForInflow = model.InflowItems;
            return View(model.InflowItems);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InflowMultiAction(InflowView model, string value)
        {
            //Dictionary<int, List<InflowView>> testek = model.GetInflowsList();

            //if (Request.Form["AddInflow"] != null)
            //{
            //}
            //if (Request.Form["EditInflow"] != null)
            //{
            //}

            // //GetInflowView();

            return View("Index", model.InflowItems);
        }

        public ActionResult GetOrderDetailsForInflow(int orderId)
        {
            List<InflowItem> items = ordersForInflow.Where(k => k.Key == orderId).SelectMany(k => k.Value).ToList();
            
            return PartialView("_InflowList", items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewInflow(int hiddenOrderId, string InflowId, string Invoice, List<int> hiddenAssetId, List<decimal> Qty, List<decimal> Value, List<decimal>Vat )//[ModelBinder(typeof(DictionaryBinder))] InflowView model)
        {

            Dictionary<int,decimal> inflowItemValues = new Dictionary<int,decimal>();
            Dictionary<int, decimal> inflowItemQtys = new Dictionary<int, decimal>();
            Dictionary<int, decimal> inflowItemVats = new Dictionary<int, decimal>();

            int i = 0;
            foreach (int item in hiddenAssetId)
            {
                inflowItemValues[item] = Value[i];
                inflowItemQtys[item] = Qty[i];
                inflowItemVats[item] = Vat[i++];
            }            

            inflow.Add(InflowId, Invoice);

            inflowItem.Add(inflowItemValues, inflowItemQtys, inflowItemVats, hiddenOrderId);

            return View("Index");
        }


    }
}