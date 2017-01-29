using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Dostawca")]
        public string Name { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10)]
        public string Nip { get; set; }

        [MaxLength(30)]
        [Display(Name = "Adres")]
        public string Address { get; set; }

        //[RegularExpression(@"\d{2}-\d{3}")]
        [StringLength(6, MinimumLength = 6)]
        [Display(Name = "Kod pocztowy")]
        public string PostCode { get; set; }

        [MaxLength(50)]
        [Display(Name = "Miasto")]
        public string City { get; set; }

        [MaxLength(50)]
        [Display(Name = "Telefon")]
        public string Phone { get; set; }
        [MaxLength(50)]

        public string Email { get; set; }
        public string AdditionatDetails { get; set; }
        public string Message { get; set; }
    }
}