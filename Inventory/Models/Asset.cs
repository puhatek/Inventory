using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Models
{
    //[Table("Assets")]
    public class Asset
    {
        public int Id { get; set; }

        [MaxLength(80)]
        [Required]
        [Display(Name = "Towar")]
        public string Name { get; set; }

        [MaxLength(10)]
        [Required]
        [Display(Name = "Indeks")]
        public string Index { get; set; }

        [MaxLength(10)]
        [Required]
        [Display(Name = "J.M.")]
        public string UnitMeasure { get; set; }

        [Required]
        public decimal Vat { get; set; }

        [Display(Name = "Apeta")]
        public bool isMedicine { get; set; }
        public int StockId { get; set; }

        //Navigation properties
        public virtual Stock Stocks { get; set; }
    }

}