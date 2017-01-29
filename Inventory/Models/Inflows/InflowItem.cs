using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models.Inflows
{
    //[Table("InflowItems")]
    public class InflowItem
    {
        MagazynDbContext db = new MagazynDbContext();
        
        [Key]
        [Column(Order = 1)]
        public int InflowId {get;set;}

        [Key]
        [Column(Order = 2)]
        public int AssetId { get; set; }

        [Key]
        [Column(Order = 3)]
        public int OrderId { get; set; }

        [NotMapped]
        public int SupplierId { get; set; }

        [NotMapped]
        [Display(Name = "Numer PZ")]
        [RegularExpression(@"^(20[0-9]{2}/)([0][1-9]|[1][0-2])(/\w[1-9]/)((0{2}[1-9])|(0[1-9][0-9])|[1-9][0-9]{2})$")]
        [MaxLength(25)]
        public string InflowNumber { get; set; }

        [Display(Name = "Numer FA")]
        [NotMapped]
        public string Invoice { get; set; }

        [Display(Name = "Dostawca")]
        [NotMapped]
        public string SupplierName {get;set;} 
        [NotMapped]
        public DateTime OrderDate {get;set;}

        [Display(Name = "Towar")]
        [NotMapped]
        public string AssetName {get;set;}

        [Display(Name = "Indeks")]
        [NotMapped]
        public string AssetIndex { get; set; }

        [Display(Name = "Ilość")]
        [NotMapped]
        public decimal Qty { get; set; }

        [Display(Name = "I.M.")]
        [NotMapped]
        public string UM { get; set; }

        [Display(Name = "Wartość")]
        [NotMapped]
        public decimal Value { get; set; }

        [Display(Name = "Cena")]
        [NotMapped]
        public decimal Price { get; set; }


        [NotMapped]
        public decimal Vat { get; set; }



        public virtual Inflow Inflows { get; set; }
        public virtual Asset Assets { get; set; }
        public virtual Order Orders { get; set; }



        internal void Add(Dictionary<int, decimal> inflowItemValues, Dictionary<int, decimal> inflowItemQtys, Dictionary<int, decimal> inflowItemVats,  int OrderId )
        {

            List<OrderItem> orderItem = db.OrderItems.Where(a => a.OrderId == OrderId).Select(a => a).ToList();

            //OrderItem newItem = new OrderItem();

            foreach (OrderItem item in orderItem)
            {
                if (item.Qty != inflowItemQtys[item.AssetId])
                {
                    item.Qty = inflowItemQtys[item.AssetId];       
                }
                item.Value = (float)inflowItemValues[item.AssetId];
                item.Vat = inflowItemVats[item.AssetId];
                item.CompletitionDate = DateTime.Now;
                db.SaveChanges();
            }

            

            //sprawdz czy qty sie zgadza, jesli nie to update i wpis do Log
 
        }
    }
}