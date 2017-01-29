using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Inventory.Models.Inflows
{
    public class InflowView
    {
        MagazynDbContext db = new MagazynDbContext();

        public Dictionary<int, List<InflowItem>> InflowItems { get; set; }

        public Inflow Inflow { get; set; }


        public InflowView()
        {
            InflowItems = new Dictionary<int, List<InflowItem>>();            
            Inflow = new Inflow();

            using (SqlConnection conn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("aa_getOrdersForInflow", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataReader reader = cmd.ExecuteReader();


                    while (reader.Read())
                    {
                        if (!InflowItems.ContainsKey(Convert.ToInt32(reader["OrderId"])))
                        {
                            InflowItems.Add(
                            (int)reader["OrderId"],
                            new List<InflowItem>());
                        }

                        InflowItems[(int)reader["OrderId"]].Add(new InflowItem()
                        {
                            SupplierId = (int)reader["SuppierId"],
                            SupplierName = Convert.ToString(reader["Supplier"]),
                            OrderDate = (DateTime)reader["Sent"],
                            AssetName = Convert.ToString(reader["Asset"]),
                            AssetIndex = Convert.ToString(reader["AssetIndex"]),
                            AssetId = (int)reader["AssetId"],
                            Qty = (decimal)reader["Qty"],
                            UM = Convert.ToString(reader["UM"]),
                            //Value = 0,
                            Vat = Convert.ToDecimal(reader["Vat"]),
                            InflowNumber = GetNextInflowIndex(),
                            Invoice = string.Empty
                            
                        });

                    }
                }

            }
        }

        private string GetNextInflowIndex()
        {
            string lastInflowIndex = db.Inflows.OrderBy(i => i.Id).Select(i => i.InflowNumber).FirstOrDefault();
            string[] words;
            words = lastInflowIndex.ToString().Split('/');
    
            int indxVal = Convert.ToInt32(words[3]);
            indxVal++;
            string nextInflowIndex = string.Format("{0}/{1}/{2}/{3}", words[0], words[1], words[2], indxVal.ToString("000"));
            return nextInflowIndex;
        }


    }
}