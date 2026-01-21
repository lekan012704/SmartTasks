using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Order
{
    public class OrderItemDto
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // Optional if you want dimensions
        public double PackageLength { get; set; } = 12;
        public double PackageWidth { get; set; } = 10;
        public double PackageHeight { get; set; } = 10;
        public int CategoryId { get; set; } = 1; // fallback
        public double Weight { get; set; } = 1;
    }

}