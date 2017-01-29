using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public class OperationLog
    {
        //primitive properties
        public int Id { get; set; }
        public int OperationTypeId { get; set; }
        public int LogDbItemId { get; set; }
        public int ItemId { get; set; }
        public DateTime OperationDate { get; set; }
        public int UserId { get; set; }

        //navigation properties
        public virtual OperationType OperationTypes { get; set; }
        public virtual LogDbItem LogDbItems { get; set; }
    }

    public class OperationType 
    {
        public int Id { get; set; }
        public string Name { get; set; } //update, delete operation
    }

    public class LogDbItem
    {
        public int Id { get; set; }
        public string Name { get; set; } //asset, supplier, orderitem
    }

}