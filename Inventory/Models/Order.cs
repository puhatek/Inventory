using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public class Order
    {
        //Primitive properties
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public int OrderStatus { get; set; }
        //Navigation properties
        public virtual Supplier Suppliers { get; set; }
    }
}