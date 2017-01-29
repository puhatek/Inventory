using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Models
{
    [NotMapped]
    public class OrderView
    {
        MagazynDbContext db = new MagazynDbContext();

        public IEnumerable<OrderItem> OrderItems {get; set;}
        public OrderNewItem NewItem { get; set; }

        public List<SelectListItem> Options
        {
            get { return _Options; }
            set
            {
                _Options = new List<SelectListItem>(){
                    new SelectListItem { Text = "Wybierz opcję", Value ="BulkActions"},
                    new SelectListItem { Text = "Zrealizuj", Value = "Complete"},
                    new SelectListItem { Text = "Usuń" , Value = "Remove"}
                };
            }
        }

        private List<SelectListItem> _Options;

        public IEnumerable<OrderItem> GetItemsList()
        {
            //var orderItems = db.OrderItems.Select(a => a).ToList();
            List<OrderItem> OrderItems = new List<OrderItem>();

            using (SqlConnection conn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("aa_getOrdeList", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataReader reader = cmd.ExecuteReader();



                    while(reader.Read())
                    {
                        OrderItems.Add(new OrderItem{
                            AssetName = reader.GetString(0),
                            AssetId = reader.GetInt32(1),
                            OrderId = reader.GetInt32(2),
                            UM = reader.GetString(3),
                            Qty = reader.GetDecimal(4),
                            isMedicine = reader.GetBoolean(5),
                            Supplier = reader.GetString(6),
                            Status = reader.GetString(7),
                            StatusPriority = reader.GetInt32(8),
                            InsertDate = reader.GetDateTime(9),
                            ExtraAssetDescription = reader.GetString(10)
                        });

                    }
                }
            }

            #region może śmieć
            //OrderItems = from i in orderItems
            //             select new OrderView
            //             {
            //                 AssetName = db.OrderItems //db.Assets
            //                    .Where(a => a.Id == i.Id)
            //                    .Select(a => a.Name).FirstOrDefault(),
            //                 AssetId = i.Id,
            //                 OrderId = i.OrderId,
            //                 UM = db.OrderItems //db.Assets
            //                    .Where(a => a.Id == i.Id)
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
            return OrderItems;
        }

        public void AddItem(OrderNewItem model, int orderId, string status)
        {
            OrderItem orderItem = new OrderItem();

            orderItem.OrderId = orderId;
            orderItem.AssetId = db.Assets.Where(a => a.Name == model.AssetName).Select(a => a.Id).FirstOrDefault(); //db.Assets.Where(
            orderItem.Qty = (int)model.Qty;
            orderItem.isMedicine = model.isMedicine;
            orderItem.InsertDate = DateTime.Now;
            orderItem.Status = status;// status.GetDbStatusName(StatusEn.awaiting);
            orderItem.StatusPriority = 1;
            orderItem.ExtraAssetDescription = model.ExtraAssetDescription == null ? string.Empty : model.ExtraAssetDescription;
            try
            {
                db.OrderItems.Add(orderItem);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }

        public IEnumerable<OrderItem> Search(string term)
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

            OrderItems = from i in orderItems
                         select new OrderItem
                        {
                            AssetName = db.Assets
                               .Where(a => a.Id == i.AssetId)
                               .Select(a => a.Name).FirstOrDefault(),
                            AssetId = i.AssetId,
                            OrderId = i.OrderId,
                            UM = db.Assets
                               .Where(a => a.Id == i.AssetId)
                               .Select(a => a.UnitMeasure).FirstOrDefault(),
                            Qty = i.Qty,
                            isMedicine = i.isMedicine,
                            Supplier = db.Suppliers
                               .Where(s => s.Id == db.Orders.Where(o => o.Id == i.OrderId).Select(o => o.SupplierId).FirstOrDefault())
                               .Select(s => s.Name).FirstOrDefault(),
                            Status = i.Status,
                            StatusPriority = i.StatusPriority,// status.GetDbStatusPriority(i.Status),
                            InsertDate = i.InsertDate,
                            ExtraAssetDescription = i.ExtraAssetDescription
                        };
            return OrderItems;

        }
    }
}