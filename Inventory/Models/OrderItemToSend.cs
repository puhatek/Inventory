using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public class OrderItemToSend
    {
        [JsonProperty("orderId")]
        public int OrderId { get; set; }
        [JsonProperty("assetId")]
        public int AssetId { get; set; }
    }
}