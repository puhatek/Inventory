using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using Inventory.Controllers;
using System.Text;
using System.Net.Mail;
using System.Web.Configuration;

namespace Inventory.Models
{
    public class MailingView
    {
        private MagazynDbContext db = new MagazynDbContext();
        public IEnumerable<OrderItem> OrderItems { get; set; }
        public Message Message { get; set; }

        private OrderStatus updateOrderStatus = new OrderStatus();

        static string STATUS_PENDING = Status.GetDbStatusName(StatusEn.pending);

        public IEnumerable<OrderItem> GetMessageDetails(List<OrderItem> orderItems, int? orderId)
        {
            return from oi in orderItems
                   join a in db.Assets
                   on oi.AssetId equals a.Id
                   join o in db.Orders
                   on oi.OrderId equals o.Id
                   join s in db.Suppliers
                   on o.SupplierId equals s.Id
                   where oi.OrderId == orderId && oi.Status == STATUS_PENDING
                   select new OrderItem
                   {
                       //określenie parametrów OrderItem
                       OrderId = oi.OrderId,
                       AssetName = a.Name,
                       Qty = oi.Qty,
                       UM = db.Assets.Where(assets => assets.Id == oi.AssetId)
                                     .Select(assets => assets.UnitMeasure)
                                     .FirstOrDefault(),
                       SetMailingDate = (DateTime)oi.SetMailingDate
                   };

        }

        public MailingView GetMessageGroups(List<OrderItem> orderItems)
        {
            return new MailingView()
            {
                OrderItems = from i in orderItems
                             join o in db.Orders
                             on i.OrderId equals o.Id
                             join s in db.Suppliers
                             on o.SupplierId equals s.Id
                             where i.SetMailingDate != null && i.Status == STATUS_PENDING
                             group i by new { i.OrderId, s.Name } into categoryGroup
                             select new OrderItem
                             {
                                 //określenie parametrów OrderItem
                                 OrderId = categoryGroup.Key.OrderId,
                                 Supplier = categoryGroup.Key.Name,
                             },
                Message = new Message()
                {
                    //określenie parametrów Message
                    SupplierId = -1,
                    MessageText = string.Empty,
                    Recipient = string.Empty,
                    OrderItems = new List<OrderItem>()
                }
            };
        }

        public List<OrderItemToSend> GetOrderItemToSend(List<OrderItem> orderItems)
        {
            return (from oi in orderItems
                    select new OrderItemToSend()
                    {
                        AssetId = db.Assets.Where(a => a.Name == oi.AssetName).Select(a => a.Id).FirstOrDefault(),
                        //AssetId = db.OrderItems.Where(a => a.Name == oi.AssetName).Select(a => a.Id).FirstOrDefault(),
                        OrderId = oi.OrderId
                    }).ToList<OrderItemToSend>();
        }



        public async Task UpdateAssetStatusAsync(List<OrderItem> orderItems, string status)
        {
            var Ids = GetOrderItemToSend(orderItems);

            List<int> assets = Ids.Select(a => a.AssetId).ToList();
            List<int> orders = Ids.Select(a => a.OrderId).ToList();

            int updatedItems = await UpdateDateInDBAsync(orders, assets, status);

            #region jakby śmieć
            //IQueryable<OrderItem> updatedEntity = from oi in db.OrderItems
            //                                      where orders.Contains(oi.OrderId)
            //                                      && assets.Contains(oi.AssetId)
            //                                      select oi;
            //
            //updatedEntity.ToList().ForEach(a =>
            //{
            //    a.Status = status;
            //    switch (status)
            //    {
            //        case "Do wysłania":
            //            a.SetMailingDate = DateTime.Now;
            //            break;
            //        case "Wysłane":
            //            a.OrderDate = DateTime.Now;
            //            break;
            //    }
            //});
            //
            //foreach (OrderItem item in updatedEntity)
            //{
            //    db.Entry(item).State = EntityState.Modified;
            //}
            //db.SaveChanges();
            #endregion
            if (updatedItems > 0)
            {
                foreach (int orderId in orders.Distinct())
                {
                    updateOrderStatus.Update(orderId);
                }
            }
        }

        public async Task<int> UpdateDateInDBAsync(List<int> orders, List<int> assets, string status)
        {
            int i = 0;
            IQueryable<OrderItem> updatedEntity = from oi in db.OrderItems
                                                  where orders.Contains(oi.OrderId)
                                                  && assets.Contains(oi.AssetId)
                                                  select oi;
            updatedEntity.ToList().ForEach( a => 
            {
                i++;
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
            return i;
        }


        //public async Task<string> SendOrderAsync(Message model)
        public async Task<string> SendOrderAsync(Message model)
        {
            //string sendRespons;

            StringBuilder textWithoutOrderTable = new StringBuilder(db.Suppliers.Where(s => s.Id == model.SupplierId).Select(s => s.Message).FirstOrDefault().Replace(@"\n", @"<br/>"));
            //MailMessage message = new MailMessage(
            //    from: WebConfigurationManager.AppSettings["FromAddress"],
            //    to: model.Recipient
            //    );

            string sendRespons = await SendMessage(model.Recipient, model.Subject, textWithoutOrderTable.Append(model.MessageText).ToString());


            //message.Subject = "gadmed - zamówienie";
            //message.Body = textWithoutOrderTable.Append(model.MessageText).ToString();
            //message.IsBodyHtml = true;
            //
            //SmtpClient smtpClient = new SmtpClient(WebConfigurationManager.AppSettings["host"]);
            //smtpClient.EnableSsl = true;
            //
            //try
            //{
            //    await smtpClient.SendMailAsync(message);
            //}
            //catch (Exception ex)
            //{
            //    sendRespons = ex.Message;
            //}
            return sendRespons;
        }

        public async Task<string> SendMessage(string Recipient, string Subject, string MessageText)
        {
            string sendRespons = "Sent";

            MailAddress _from = new MailAddress(WebConfigurationManager.AppSettings["FromAddress"]);
            MailAddress _to = new MailAddress(Recipient);
           
            MailMessage message = new MailMessage(
                from: WebConfigurationManager.AppSettings["FromAddress"],
                to: Recipient
                );
            message.Subject = Subject;
            message.Body = MessageText;
            message.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient(WebConfigurationManager.AppSettings["host"]);
            smtpClient.EnableSsl = true;

            try
            {
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                sendRespons = ex.Message;
            }
            return sendRespons;
        }

    }
}