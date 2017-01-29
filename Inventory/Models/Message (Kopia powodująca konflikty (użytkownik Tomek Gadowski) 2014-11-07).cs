using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace Inventory.Models
{
    public class Message
    {
        public int SupplierId { get; set; }
        public string Recipient { get; set; }

        public string MessageText { get; set; }

        public List<OrderItem> OrderItems { get; set; }
    }
}