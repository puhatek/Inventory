using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    //[Table("OrderItems")]
    public class OrderItem
    {
        //Primitive properties
        [Key]
        [Column(Order = 1)]
        public int AssetId { get; set; }

        [Key]
        [Column(Order = 2)]     
        public int OrderId { get; set; }

        [NotMapped]
        [Display(Name = "Towar")]    
        public string AssetName { get; set; }

        [Display(Name="Uwagi")]
        [StringLength(150)]
        public string ExtraAssetDescription { get; set; }

        [NotMapped]
        [Display(Name = "J.M.")]
        public string UM { get; set; }

        [Display(Name = "Ilość")]
        public decimal Qty { get; set; }

        [Display(Name = "Cena")]
        [NotMapped]
        public Single Price { get; set; }

        [Display(Name = "Wartość")]      
        public Single Value { get; set; }

        public decimal Vat { get; set; }

        [Display(Name = "Apteka")]
        public bool isMedicine { get; set; }

        [NotMapped]
        [Display(Name = "Kontrahent")]
        public string Supplier { get; set; }

        public string Status { get; set; }

        public int StatusPriority { get; set; }

        [Display(Name = "Zgłoszono")]
        [DisplayFormat(DataFormatString = "{0:g}", ApplyFormatInEditMode= true)]
        [DataType(DataType.DateTime)]
        //[myCustomValidation("Data zgłoszenia")]
        public DateTime InsertDate { get; set; }

        [Display(Name = "Przyjęto do realizacji")]
        public DateTime? SetMailingDate { get; set; }

        [Display(Name = "Wysłano")]
        public DateTime? OrderDate { get; set; }

        [Display(Name = "Zrealizowano")]
        public DateTime? CompletitionDate { get; set; }

        //Navigation properties
        [ForeignKey("AssetId")]
        public virtual Asset Assets { get; set; }
        
        [ForeignKey("OrderId")]        
        public virtual Order Orders { get; set; }


    }
}