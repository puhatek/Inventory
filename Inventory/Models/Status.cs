using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public enum StatusEn
    {
        awaiting,
        pending,
        sent,
        completed
    }

    public class Status
    {
        public static Dictionary<List<string>, int> Asset = new Dictionary<List<string>, int>();
        static Status()
        {
            Asset.Add(new List<string> { "awaiting", "Oczekujące" }, 1);
            Asset.Add(new List<string> { "pending", "Do wysłania" }, 2);
            Asset.Add(new List<string> { "sent", "Wysłane" }, 3);
            Asset.Add(new List<string> { "completed", "Zrealizowane" }, 4);
        }

        /// <summary>
        /// Asset receive keys: awaiting, ordered, added
        /// </summary>
        private MagazynDbContext db = new MagazynDbContext();
        /// <summary>
        /// Gets an English status name and returns a Polish equivalent from db 
        /// </summary>
        /// <param name="baseName">Base English statuses: awaiting, pending, sent, completed</param>
        /// <returns>Status name kept in data base</returns>
        public static string GetDbStatusName(StatusEn baseName)
        {
            return GetAssetStatusDbValues(Convert.ToString(baseName.ToString()))[0];
        }

        public static int GetDbStatusPriority(string baseName)
        {
            int result;
            return int.TryParse(GetAssetStatusDbValues(baseName)[1], out result) ? result : 0;
        }

        private static string[] GetAssetStatusDbValues(string BaseName)
        {
            string[] dbValue = { string.Empty, string.Empty };
            foreach (var item in Asset)
            {

                if (item.Key[0] == BaseName)
                {
                    return new string[] { item.Key[1], item.Value.ToString() };
                }

                if (item.Key[1] == BaseName)
                {
                    return new string[] { item.Key[1], item.Value.ToString() };
                }

            }
            return dbValue;
        }

        public bool hasSupplier(int SupplierId, StatusEn _Status)
        {
            string _status = GetDbStatusName(_Status);
            //int countStatus = (from oi in db.OrderItems
            //                  join o in db.Orders
            //                  on oi.OrderId equals o.Id
            //                  where o.SupplierId == SupplierId
            //                     && oi.Status == _status
            //                  orderby oi.OrderId
            //                  select oi.AssetId).FirstOrDefault();
            return (from oi in db.OrderItems
                    join o in db.Orders
                    on oi.OrderId equals o.Id
                    where o.SupplierId == SupplierId
                       && oi.Status == _status
                    orderby oi.OrderId
                    select oi.AssetId).FirstOrDefault().Equals(0) ? false : true;
        }
    }
}