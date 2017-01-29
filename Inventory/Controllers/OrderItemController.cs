#define localDB
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Inventory.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Data.Entity.Validation;

namespace Inventory.Controllers
{
    [Authorize]
    public class OrderItemController : Controller
    {
        private MagazynDbContext db = new MagazynDbContext();
        private ApplicationDbContext appDb = new ApplicationDbContext();
        private Status status = new Status();
        private OrderStatus orderStatus = new OrderStatus();

        private static OrderView ov = new OrderView();
        private static MailingView mv = new MailingView();

        private bool _isAdmin;
        protected bool isInAdminRole
        {
            get
            {
                return User.IsInRole("Admin");
            }
        }

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

        // -------------------------------------- CRUD operations --------------------------------------
        #region CRUD operations
        [HttpGet]
        public ActionResult Index()
        {
            return View(GetOrderViewModel());
        }

        public ActionResult UpdateOrderList(string partialToReturn)
        {
            OrderView model = GetOrderViewModel();
            return partialToReturn == "body" ? PartialView("_Body", model.OrderItems) : PartialView("_AddItem", model.NewItem);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(OrderNewItem model)
        {
            if (ModelState.IsValid)
            {
                int supplierId = GetSupplierIdFromCurrentOrder(model.Supplier);

                int orderId = GetOrder(supplierId, ORDER_STATUT_AWAITING);

                if (orderId == 0)
                {
                    orderId = GetOrder(supplierId, ORDER_STATUT_PENDING);
                    if (orderId != 0)
                    {
                        //send an information to admin. Only admin can confirm and add to order a new item with pending status.                       
                        return View(model);
                    }
                }
                if (orderId == 0)
                {
                    orderId = SetSupplierForNewOrder(supplierId);
                }

                AddOrderItem(model, orderId);

                var updatedModel = GetOrderViewModel();

                return Json(new object[] { string.Empty, true }, JsonRequestBehavior.AllowGet);
                //return PartialView("_Body", updatedModel.OrderItems);
            }

            //AssetName and Qty are the only controlls needed to be validate
            List<List<string>> err = new List<List<string>>();
            foreach (var item in ModelState)
            {
                if ((item.Key == "AssetName" || item.Key == "Qty") && (item.Value.Errors.Count > 0))
                {
                    err.Add(new List<string> { item.Key, item.Value.Errors[0].ErrorMessage });
                }
            }
            return Json(new object[] { err, false }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit([Bind(Include = "AssetId, OrderId, AssetName, UM, Supplier, ExtraAssetDescription")] OrderItem model, bool isConditionalEdit)
        {
            OrderItem _model = db.OrderItems.Find(model.AssetId, model.OrderId);
            return View(_model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(OrderItem model, int BaseSupplierId)
        {
            if (ModelState.IsValid)
            {
                int updatedSupplierId = GetSupplierIdFromCurrentOrder(model.Supplier);

                if (BaseSupplierId != updatedSupplierId)
                {
                    if (status.hasSupplier(updatedSupplierId, StatusEn.awaiting))
                    {
                        //wtedy bierzemy id tego zamówienia i dodajemy je w tabeli orderitem do kolumny orderid 
                        UpdateOrderItem(model, false, updatedSupplierId);
                    }
                    else
                    {
                        //wtedy jako że nie ma otwartego zamówienia tworzymy nową krotkę w tabeli. id tej krotki 
                        //wstawiamy do tabeli orderitem do kolumn orderid
                        UpdateOrderItem(model, true, updatedSupplierId);
                    }
                    return RedirectToAction("Index");
                }

                UpdateOrderItem(model, false, updatedSupplierId);
                return RedirectToAction("Index");
            }
            return View();
        }

        private void UpdateOrderItem(OrderItem model, bool isNewOrderToAdd, int updateSupplierId)
        {
            int baseOrderId = model.OrderId;
            model.InsertDate = DateTime.Now;
            if (isNewOrderToAdd)
            {
                model.OrderId = SetSupplierForNewOrder((int)updateSupplierId);
            }
            else
            {
                model.OrderId = db.Orders.Where(o => o.SupplierId == (int)updateSupplierId).Select(o => o.Id).First();
            }
            var sqlCommand = @"update dbo.OrderItems 
                               set AssetId = @assetId, OrderId = @orderId, Qty = @qty, isMedicine = @isMedicine, InsertDate = @insertDate, ExtraAssetDescription = @extraAssetDescription 
                               where AssetId = @assetId and OrderId = @baseOrderId";
            db.Database.ExecuteSqlCommand(sqlCommand,
                                            new SqlParameter("@assetId", model.AssetId),
                                            new SqlParameter("@orderId", model.OrderId),
                                            new SqlParameter("@qty", model.Qty),
                                            new SqlParameter("@isMedicine", model.isMedicine),
                                            new SqlParameter("@insertDate", model.InsertDate),
                                            new SqlParameter("@extraAssetDescription", model.ExtraAssetDescription == null ? string.Empty : model.ExtraAssetDescription),
                                            new SqlParameter("@baseOrderId", baseOrderId)
                );
        }

        [HttpGet]
        public ActionResult Delete(int orderId, int assetId)
        {
            string currentStatus = db.OrderItems.Where(oi => oi.AssetId == assetId).Select(oi => oi.Status).First();

            IQueryable<OrderItem> orderItem = db.OrderItems.Select(oi => oi);
            OrderItem entiry = orderItem
                .Where(i => i.AssetId == assetId)
                .Where(i => i.OrderId == orderId)
                .Select(i => i).FirstOrDefault();
            db.Entry(entiry).State = EntityState.Deleted;
            db.SaveChanges();

            orderStatus.Update(orderId);

            //UpdateOrderStatus(orderId);
            //if (orderStatus.IsToUpdate(orderId, currentStatus == "Oczekujące" ? "Do wysłania" : "Wysłane"))
            //{
            //    orderStatus.Update(orderId);
            //}
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        #endregion

        // -------------------------------------- CRUD operations --------------------------------------        
        #region Helpers

        private void UpdateOrderStatus(int orderId)
        {
            //var oderId = new SqlParameter("@oderId", orderId);
            try
            {
                var result = db.Database
                               .SqlQuery(typeof(int), "exec aa_updateOrderStatus @oderId", new SqlParameter("@oderId", orderId));
            }
            catch (Exception ex)
            {

            }                      
        }

        private int GetOrder(int supplierId, int status)
        {
            return db.Orders.OrderByDescending(o => o.Id)
                     .Where(o => o.SupplierId == supplierId && o.OrderStatus == status)
                     .Select(o => o.Id).FirstOrDefault();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search(string term)
        {
            List<OrderItem> orderItems = (from oi in db.OrderItems
                                          join a in db.Assets
                                          on oi.AssetId equals a.Id
                                          join o in db.Orders
                                          on oi.OrderId equals o.Id
                                          join s in db.Suppliers
                                          on o.SupplierId equals s.Id
                                          where a.Name.Contains(term) || oi.InsertDate.ToString().Contains(term)
                                          || s.Name.Contains(term) || oi.Qty.ToString().Contains(term)
                                          || oi.ExtraAssetDescription.Contains(term)
                                          select oi).ToList();


            var assetResult = ov.Search(term);
            #region raczaj śmieć
            //from i in orderItems
                //              select new OrderItem
                //             {
                //                 AssetName = db.Assets
                //                    .Where(a => a.Id == i.AssetId)
                //                    .Select(a => a.Name).FirstOrDefault(),
                //                 AssetId = i.AssetId,
                //                 OrderId = i.OrderId,
                //                 UM = db.Assets
                //                    .Where(a => a.Id == i.AssetId)
                //                    .Select(a => a.UnitMeasure).FirstOrDefault(),
                //                 Qty = i.Qty,
                //                 isMedicine = i.isMedicine,
                //                 Supplier = db.Suppliers
                //                    .Where(s => s.Id == db.Orders.Where(o => o.Id == i.OrderId).Select(o => o.SupplierId).FirstOrDefault())
                //                    .Select(s => s.Name).FirstOrDefault(),
                //                 Status = i.Status,
                //                 StatusPriority = i.StatusPriority,// status.GetDbStatusPriority(i.Status),
                //                 InsertDate = i.InsertDate,
                //                 ExtraAssetDescription = i.ExtraAssetDescription
            //             }; 
            #endregion
            return PartialView("_Body", assetResult);
        }

        private OrderView GetOrderViewModel()
        {
            OrderView ov = new OrderView();
            //var orderItems = db.OrderItems.Select(a => a).ToList();// Select(oi => new { oi.AssetId, oi.OrderId, oi.InsertDate, oi.Status, oi.Qty, oi.isMedicine, oi.StatusPriority, oi.ExtraAssetDescription }).ToList();      
            
            var model = new OrderView()
            {
                OrderItems = ov.GetItemsList(),

                #region raczaj śmieć
                //from i in orderItems
                //             select new OrderItem
                //             {
                //                 AssetName = db.Assets
                //                    .Where(a => a.Id == i.AssetId)
                //                    .Select(a => a.Name).FirstOrDefault(),
                //                 AssetId = i.AssetId,
                //                 OrderId = i.OrderId,
                //                 UM = db.Assets
                //                    .Where(a => a.Id == i.AssetId)
                //                    .Select(a => a.UnitMeasure).FirstOrDefault(),
                //                 Qty = i.Qty,
                //                 isMedicine = i.isMedicine,
                //                 Supplier = db.Suppliers
                //                    .Where(s => s.Id == db.Orders.Where(o => o.Id == i.OrderId).Select(o => o.SupplierId).FirstOrDefault())
                //                    .Select(s => s.Name).FirstOrDefault(),
                //                 Status = i.Status,
                //                 StatusPriority = i.StatusPriority,// status.GetDbStatusPriority(i.Status),
                //                 InsertDate = i.InsertDate,
                //                 ExtraAssetDescription = i.ExtraAssetDescription
                //             },
                #endregion
                NewItem = new OrderNewItem()
                {
                    AssetName = string.Empty,
                    Qty = null,
                    isMedicine = false,
                    ExtraAssetDescription = string.Empty,
                    Supplier = string.Empty,
                    InsertDate = DateTime.Now
                },
                Options = new List<SelectListItem>()
            };
            return model;
        }

        /// <summary>
        /// Get the supplier's id value. If SupplierName is null get default empty supplier id. 
        /// </summary>
        /// <param name="SupplierName"></param>
        /// <returns></returns>
        private int GetSupplierIdFromCurrentOrder(string SupplierName)
        {
            if (SupplierName == null && db.Suppliers.Where(s => s.Name == "BRAK").Select(s => s.Id).Count() == 0)
            {
                db.Suppliers.Add(new Supplier { Name = "BRAK", Nip = "1234567890" });
                db.SaveChanges();
            }
            return SupplierName == null ?
                db.Suppliers.Where(s => s.Name == "BRAK").Select(s => s.Id).First() :
                db.Suppliers.Where(s => s.Name == SupplierName).Select(s => s.Id).FirstOrDefault();
        }

        private int SetSupplierForNewOrder(int supplierId)
        {
            Order order = new Order();
            order.SupplierId = supplierId;
            order.OrderStatus = ORDER_STATUT_AWAITING;
            try
            {
                db.Orders.Add(order);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            int orderId = db.Orders.OrderByDescending(o => o.Id).Select(o => o.Id).First();
            return orderId;
        }

            

        private void AddOrderItem(OrderNewItem model, int orderId)
        {
            ov.AddItem(model, orderId, STATUS_AWAITING);

            #region raczaj śmieć
            //OrderItem orderItem = new OrderItem();
            //
            //orderItem.OrderId = orderId;
            //orderItem.AssetId = db.Assets.Where(a => a.Name == model.AssetName).Select(a => a.Id).FirstOrDefault();
            //orderItem.Qty = (int)model.Qty;
            //orderItem.isMedicine = model.isMedicine;
            //orderItem.InsertDate = DateTime.Now;
            //orderItem.Status = STATUS_AWAITING;// status.GetDbStatusName(StatusEn.awaiting);
            //orderItem.StatusPriority = 1;
            //orderItem.ExtraAssetDescription = model.ExtraAssetDescription == null ? string.Empty : model.ExtraAssetDescription;
            //try
            //{
            //    db.OrderItems.Add(orderItem);
            //    db.SaveChanges();
            //}
            //catch (Exception ex)
            //{
            //    string error = ex.Message;
            //}
            #endregion
        }


        public async Task<JsonResult> AddAsset(OrderNewItem model)
        {
            string response = "done";
            string userId = appDb.Users.Where(u => u.UserName == User.Identity.Name).Select(u => u.Id).FirstOrDefault();
            //return appDb.UserStocks.Where(us => us.UserId.Equals(userId)).Select(us => us.StockId).FirstOrDefault();
            
            Asset newItem = new Asset()
            {
                Name = model.AssetName,
                Index = model.AssetIndex == null ? "AP000" : model.AssetIndex,
                UnitMeasure = model.UM == null ? string.Empty : model.UM,
                Vat = 0.0M,
                isMedicine = model.isMedicine,
                StockId = model.StockId,                
            };

            db.Assets.Add(newItem);
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                response = ex.Message;
            }
            
            if (!isInAdminRole)
            { 
                response = await mv.SendMessage("cheschire_kotek@hotmail.com", 
                              "Dodanie nowego produktu",
                              string.Format("Użytkownik {0} dodał nowy towar: {1}", User.Identity.Name, model.AssetName));
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #endregion

        // -------------------------------------- AJAX operations --------------------------------------
        #region Ajax operations
        private int GetUserStockId()
        {
            string userId = appDb.Users.Where(u => u.UserName == User.Identity.Name).Select(u => u.Id).FirstOrDefault();
            return appDb.UserStocks.Where(us => us.UserId.Equals(userId)).Select(us => us.StockId).FirstOrDefault();
        }


        public JsonResult GetStockId(string stockName)
        { 
            int stockId = db.Stocks.Where(s => s.Name == stockName).Select(s => s.Id).First();
            return Json(stockId, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AssetAutocomplete(string term)
        {
            //List<string> assety = db.OrderItems.Select(a => a.Name).ToList();

            if (User.IsInRole("Admin"))
            {
                var model = db.Assets
                    .Where(a => a.Name.Contains(term))
                    .Take(10)
                    .Select(a =>
                    new { label = a.Name }
                    );
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            else
            {
                int stockId = GetUserStockId();
                var model = db.Assets
                    .Where(a => a.Name.Contains(term))
                    .Where(a => a.StockId == stockId)
                    .Take(10)
                    .Select(a =>
                        new { label = a.Name }
                        );
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SupplierAutocomplete(string term)
        {
            var model = db.Suppliers
                .Where(a => a.Name.Contains(term))
                .Take(10)
                .Select(a =>
                    new { label = a.Name }
                    );
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult isMedicine(string AssetName)
        {
            return Json(AssetName != null ? db.Assets.Where(a => a.Name == AssetName).Select(a => a.isMedicine).FirstOrDefault() : false, JsonRequestBehavior.AllowGet);
            //return Json(AssetName != null ? db.OrderItems.Where(a => a.Name == AssetName).Select(a => a.isMedicine).FirstOrDefault() : false, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [OutputCache(NoStore=true, Duration=0, VaryByParam="*")]
        public ActionResult isAdmin()
        {
            bool t = isInAdminRole;
            return Json(isInAdminRole, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetLastSupplier(string AssetName)
        {
            var lastSupplier = (from s in db.Suppliers
                                where s.Id == (from oi in db.OrderItems
                                               join o in db.Orders
                                               on oi.OrderId equals o.Id
                                               where oi.AssetId == db.Assets.Where(a => a.Name == AssetName).Select(a => a.Id).FirstOrDefault() //db.Assets.Where(...
                                               orderby o.Id descending
                                               select o.SupplierId).FirstOrDefault()
                                select s.Name).FirstOrDefault();
            return Json(lastSupplier == null ? string.Empty : lastSupplier, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetUnitMeasure(string AssetName)
        {
            return Json(AssetName != null ? db.Assets.Where(a => a.Name == AssetName).Select(a => a.UnitMeasure).FirstOrDefault() : string.Empty, JsonRequestBehavior.AllowGet);
           // return Json(AssetName != null ? db.OrderItems.Where(a => a.Name == AssetName).Select(a => a.UnitMeasure).FirstOrDefault() : string.Empty, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult IsAssetNew(string AssetName)
        {
            return Json(IsNew(AssetName) ? true : false, JsonRequestBehavior.AllowGet);
        }

        private bool IsNew(string AssetName)
        {
            //return db.OrderItems.Where(a => a.Name == AssetName).Select(a => a.Id).FirstOrDefault().Equals(0);
            return db.Assets.Where(a => a.Name == AssetName).Select(a => a.Id).FirstOrDefault().Equals(0);
        }

        #endregion


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // -------------------------------------- Garbage --------------------------------------
        #region Garbage

        // GET: OrderItem/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderItem orderItem = db.OrderItems.Find(id);
            if (orderItem == null)
            {
                return HttpNotFound();
            }
            return View(orderItem);
        }

        // GET: OrderItem/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: OrderItem/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "AssetId,OrderId,Qty,isMedicine,Status,InsertDate,SetMailingDate,OrderDate,CompletitionDate")] OrderItem orderItem)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.OrderItems.Add(orderItem);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(orderItem);
        //}

        // GET: OrderItem/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    OrderItem orderItem = db.OrderItems.Find(id);
        //    if (orderItem == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(orderItem);
        //}

        // POST: OrderItem/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "AssetId,OrderId,Qty,isMedicine,Status,InsertDate,SetMailingDate,OrderDate,CompletitionDate")] OrderItem orderItem)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(orderItem).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(orderItem);
        //}

        // GET: OrderItem/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    OrderItem orderItem = db.OrderItems.Find(id);
        //    if (orderItem == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(orderItem);
        //}

        //// POST: OrderItem/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    OrderItem orderItem = db.OrderItems.Find(id);
        //    db.OrderItems.Remove(orderItem);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}
        #endregion


    }
}
