using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace Inventory.Models.Inflows
{
    [Table("Inflows")]
    public class Inflow
    {
        MagazynDbContext db = new MagazynDbContext();

        public int Id { get; set; }

        [Display(Name = "Numer FA")]
        [MaxLength(25)]
        public string Invoice { get; set; }

        [Display(Name = "Numer PZ")]
        [RegularExpression(@"^(20[0-9]{2}/)([0][1-9]|[1][0-2])(/\w[1-9]/)((0{2}[1-9])|(0[1-9][0-9])|[1-9][0-9]{2})$")]
        [MaxLength(25)]
        public string InflowNumber { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Data")]
        public DateTime? InflowDate { get; set; }


        internal void Add(string InflowId, string Invoice)
        {
            Inflow inflow = new Inflow();

            inflow.InflowDate = DateTime.Now;
            inflow.InflowNumber = InflowId;
            inflow.Invoice = Invoice;
            try
            {
                db.Inflows.Add(inflow);

                db.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation(
                              "Class: {0}, Property: {1}, Error: {2}",
                              validationErrors.Entry.Entity.GetType().FullName,
                              validationError.PropertyName,
                              validationError.ErrorMessage);
                    }
                }
            }
        }


    }
}