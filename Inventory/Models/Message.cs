using System;
using Inventory.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Net.Mail;
using System.Web.Configuration;
using System.Data.Entity;
using System.Linq;

namespace Inventory.Models
{
    public class Message
    {
        public MagazynDbContext db = new MagazynDbContext();

        public int SupplierId { get; set; }

        public string Recipient { get; set; }

        public string MessageText { get; set; }

        public string Subject { get; set; }

        public List<OrderItem> OrderItems { get; set; }




    }
}