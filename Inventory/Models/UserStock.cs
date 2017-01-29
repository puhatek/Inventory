using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNet.Identity.EntityFramework;

namespace Inventory.Models
{
    public class UserStock
    {
        //Primitive properties
        [Key]
        [Column(Order=0)]
        [MaxLength(128)]
        public string UserId { get; set; }
        [Key]
        [Column(Order = 1)]
        public int StockId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUsers {get;set;}
        [ForeignKey("StockId")]
        public virtual Stock Stocks { get; set; }

    }
}