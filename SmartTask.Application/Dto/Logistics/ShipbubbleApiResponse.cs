using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Logistics
{
    // Maps the top-level "data" object
    public class ShipbubbleApiResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public ShipbubbleData Data { get; set; }
    }

    // Maps the content inside "data"
    public class ShipbubbleData
    {
        [JsonProperty("request_token")]
        public string RequestToken { get; set; }

        [JsonProperty("couriers")]
        public List<ShipbubbleCourierRaw> Couriers { get; set; }
    }

    // Maps the individual items inside "couriers"
    public class ShipbubbleCourierRaw
    {
        [JsonProperty("service_code")]
        public string ServiceCode { get; set; } // We will use this as RateId

        [JsonProperty("courier_name")]
        public string CourierName { get; set; }

        [JsonProperty("service_type")]
        public string ServiceType { get; set; }

        [JsonProperty("total")]
        public decimal Total { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("delivery_eta")]
        public string DeliveryEta { get; set; }

        [JsonProperty("courier_image")]
        public string CourierImage { get; set; }
    }
}
