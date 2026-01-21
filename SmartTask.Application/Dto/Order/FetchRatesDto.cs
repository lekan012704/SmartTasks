using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Order
{
    public class FetchRatesDto
    {
        public AddressDto Sender { get; set; } = new();
        public AddressDto Receiver { get; set; } = new();

        public decimal Weight { get; set; }
        public decimal Amount { get; set; }
        public string ServiceType { get; set; } = "delivery";
        public Guid orderId { get; set; }

        // Add order items
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

        // Optional package dimension for the whole shipment
        public PackageDimension? PackageDimension { get; set; }

       
    }


    public class PackageItem
    {
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public double unit_weight { get; set; }
        public double unit_amount { get; set; }
        public int quantity { get; set; }
    }

    public class PackageDimension
    {
        public double length { get; set; }
        public double width { get; set; }
        public double height { get; set; }
    }

}
