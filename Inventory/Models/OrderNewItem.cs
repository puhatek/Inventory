using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{

    public class OrderNewItem
    {
        //[Key]
        //public int Id { get; set; }

        [Display(Name="Towar")]
        [Required()]
        public string AssetName { get; set; }

        [Display(Name="Uwagi")]
        public string ExtraAssetDescription { get; set; }

        [Display(Name="J.M.")]
        public string UM { get; set; }
        
        [Display(Name="Data zgłoszenia")]
        [DisplayFormat(DataFormatString="{g:0}", ApplyFormatInEditMode= true)]
        [DataType(DataType.DateTime)]
        public DateTime InsertDate { get; set; }

        [Display(Name="Apteka")]
        public bool isMedicine { get; set; }

        [Display(Name="Ilość")]
        [Range(1,1000,ErrorMessage="Pole Ilość musi być z zakresu od 1 do 1000")]
        [Required()]
        public decimal? Qty { get; set; }
        
        [Display(Name="Kontrahent")]        
        public string Supplier { get; set; }

        public int StockId { get; set; }

        public string AssetIndex { get; set; }
    }
}