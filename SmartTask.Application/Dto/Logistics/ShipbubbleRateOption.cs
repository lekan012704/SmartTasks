using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Logistics
{
    public class ShipbubbleRateOption
    {
        public string? RateId { get; set; }
        public string? CourierName { get; set; }
        public string? ServiceName { get; set; }
        public decimal Price { get; set; }
        public string? Currency { get; set; }
        public string? EstimatedDeliveryTime { get; set; }
        public string? CourierLogoUrl { get; set; }
    }
}
