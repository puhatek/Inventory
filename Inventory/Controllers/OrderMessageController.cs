using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Inventory.Models;
using System.Data.Entity;
using Newtonsoft.Json;
using Inventory.Models.Helpers;
using System.Net.Mail;
using System.Web.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections;
using System.IO;
using Novacode;
using System.Diagnostics;

namespace Inventory.Controllers
{
    [Authorize]
    public class OrderMessageController : Controller
    {
        MagazynDbContext db = new MagazynDbContext();
        Status status = new Status();
        OrderStatus updateOrderStatus = new OrderStatus();

        private OrderView ov = new OrderView();

        private MailingView mv = new MailingView();

        static string STATUS_AWAITING = Status.GetDbStatusName(StatusEn.awaiting);
        static string STATUS_PENDING = Status.GetDbStatusName(StatusEn.pending);
        static string STATUS_SENT = Status.GetDbStatusName(StatusEn.sent);
        static string STATUS_COMPLETED = Status.GetDbStatusName(StatusEn.completed);


        // -------------------------------------- CRUD operations --------------------------------------
        #region CRUD operations

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(int? OrderId = 0)
        {
            List<OrderItem> orderItems = await db.OrderItems.ToListAsync();

            if (Request.IsAjaxRequest() && OrderId >= 0)
            {
                var model = mv.GetMessageDetails(orderItems, OrderId);

                Session["OrderDetailsBySupplier"] = null;
                Session["OrderDetailsBySupplier"] = model.ToList();

                return PartialView("_OrderToSendDetails", model);
            }
            else
            {
                var model = mv.GetMessageGroups(orderItems);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string mailingItemIds)
        {
            List<OrderItemToSend> ids = JsonConvert.DeserializeObject<List<OrderItemToSend>>(mailingItemIds);

            UpdateAssetStatus(ids, STATUS_PENDING);// status.GetDbStatusName( StatusEn.pending));
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Helpers
        //The method updates OrderItems status
        public void UpdateAssetStatus(List<OrderItemToSend> Ids, String status)
        {
            List<int> assets = Ids.Select(a => a.AssetId).ToList();
            List<int> orders = Ids.Select(a => a.OrderId).ToList();



            IQueryable<OrderItem> updatedEntity = from oi in db.OrderItems
                                                  where orders.Contains(oi.OrderId)
                                                  && assets.Contains(oi.AssetId)
                                                  select oi;

            updatedEntity.ToList().ForEach(a =>
            {
                a.Status = status;
                switch (status)
                {
                    case "Do wysłania":
                        a.SetMailingDate = DateTime.Now;
                        break;
                    case "Wysłane":
                        a.OrderDate = DateTime.Now;
                        break;
                }
            });

            foreach (OrderItem item in updatedEntity)
            {
                db.Entry(item).State = EntityState.Modified;
            }
            db.SaveChanges();

            foreach (int orderId in orders.Distinct())
            {
                //if (updateOrderStatus.IsToUpdate(orderId, status))
                //{
                updateOrderStatus.Update(orderId);
                //}
            }

        }




        [HttpPost]
        [MultipleButton(ButtonName = "clickButton", ButtonValue = "Pdf")]
        public ActionResult CreatePdfMedicineOrderSheet()
        {
            #region pdf itextsharp
            //Document doc = new Document();
            //string path = Server.MapPath("PDFs");
            //PdfWriter.GetInstance(doc, new FileStream(path + "/Zamowienie.pdf", FileMode.Create));
            //
            //PdfPTable orderTable = new PdfPTable(3);
            //
            //List<OrderItem> medicines = (List<OrderItem>)Session["OrderDetailsBySupplier"];
            //var datas = (from m in medicines
            //                     select new
            //                     {
            //                         AssetName = m.AssetId,
            //                         Qty = m.Qty,
            //                         UM = m.UM
            //                     });
            //datas.ToList().ForEach(a => {
            //    orderTable.AddCell(a.AssetName.ToString());
            //    orderTable.AddCell(a.Qty.ToString());
            //    orderTable.AddCell(a.UM.ToString());
            //});
            //
            //doc.Open();
            //doc.Add(orderTable);
            //doc.Close();
            #endregion

            string fileName = Path.Combine(Server.MapPath("/Content"), "Zamowienie.docx");

            using (DocX doc = DocX.Create(fileName))
            {

                List<OrderItem> medicines = (List<OrderItem>)Session["OrderDetailsBySupplier"];

                Table orderTable = doc.AddTable(medicines.Count, 3);
                var datas = (from m in medicines
                             select new
                             {
                                 AssetName = m.AssetName,
                                 Qty = m.Qty,
                                 UM = m.UM
                             }).ToList();

                for (int i = 0; i < datas.Count; i++)
                {
                    orderTable.Rows[i].Cells[0].Paragraphs.First().Append(datas[i].AssetName);
                    orderTable.Rows[i].Cells[1].Paragraphs.First().Append(datas[i].Qty.ToString());
                    orderTable.Rows[i].Cells[2].Paragraphs.First().Append(datas[i].UM);
                }
                doc.InsertTable(orderTable);
                doc.Save();

            }
            Process.Start(fileName);

            return View();
        }

        #endregion

        #region Methods connected with posting message
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(ButtonName = "clickButton", ButtonValue = "wyślij")]
        public async Task<JsonResult> Send([ModelBinder(typeof(MessageBinder))] Message model)
        {
            List<OrderItem> orderItem = (List<OrderItem>)Session["OrderDetailsBySupplier"];

            await mv.UpdateAssetStatusAsync(orderItem, STATUS_SENT);

            string sendRespons = await mv.SendOrderAsync(model);

            string dupa = string.Empty;

            //value is passing to a function set on onSuccess event of Ajax.BeginForm in the Index view. 
            return Json(new { RedirectTo = Url.Action("Index"), SendRespons = sendRespons });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(ButtonName = "clickButton", ButtonValue = "Szablon wiadomości")]
        public ActionResult SetMessageTemplate(Message model)
        {
            return PartialView();
        }
        #endregion

        #region Ajax methods

        [ValidateInput(false)]
        public ActionResult SetMessageTemplate(string NewTextMessage, int OrderId)
        {
            //int startFrom = NewTextMessage.TrimStart().IndexOf("<t");
            int endTableTag = NewTextMessage.IndexOf("</t");

            string messageBeforeOrderTabel = NewTextMessage.Substring(0, NewTextMessage.TrimStart().IndexOf("<t"))
                                                           .Replace("<div>", "<br>")
                                                           .Replace("</div>", "");
            //string messageAfterOrderTabel = NewTextMessage.Substring(endTableTag + 8); // 8 it's the lenght of '</table>'

            int supplierId = db.Orders.Where(o => o.Id == OrderId).Select(o => o.SupplierId).First();
            Supplier messageEntity = db.Suppliers.Where(s => s.Id == supplierId).Select(s => s).First();
            messageEntity.Message = messageBeforeOrderTabel;
            db.Entry(messageEntity).State = EntityState.Modified;
            db.SaveChanges();

            return PartialView();
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetMessageRecipient(int OrderId)
        {
            var supplier = db.Suppliers.Where(s => s.Id == db.Orders.Where(o => o.Id == OrderId).Select(o => o.SupplierId).FirstOrDefault()).Select(s => s).ToList();

            Message message = new Message()
            {
                SupplierId = db.Suppliers.Where(s => s.Id == db.Orders.Where(o => o.Id == OrderId).Select(o => o.SupplierId).FirstOrDefault()).Select(s => s.Id).FirstOrDefault(),
                Recipient = supplier[0].Email
            };
            return PartialView("_MessageContainer", message);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetMessageContent(int OrderId)
        {
            Message message = new Message()
            {
                MessageText = db.Suppliers.Where(s => s.Id == db.Orders.Where(o => o.Id == OrderId).Select(o => o.SupplierId).FirstOrDefault()).Select(s => s.Message).FirstOrDefault().Replace(@"\n", @"<br/>")
            };
            return PartialView("_MessageContainerText", message);
        }

        [HttpGet]
        public ActionResult ContainsMedicine(int OrderId)
        {
            var containsMedicine = (from oi in db.OrderItems
                                    //join a in db.Assets 
                                    //on oi.AssetId equals a.Id
                                    where oi.OrderId == OrderId
                                    select oi).Count(a => a.isMedicine == true); //a
            return containsMedicine.Equals(0) ? Json(false, JsonRequestBehavior.AllowGet) : Json(true, JsonRequestBehavior.AllowGet);
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
    }

    public class OrderStatus : Status
    {
        MagazynDbContext db = new MagazynDbContext();

        public void Update(int orderId)
        {
            var result = db.Database
                           .SqlQuery<int>("exec aa_updateOrderStatus @oderId", new SqlParameter("@oderId", orderId)).ToList();

        }

    }

}


