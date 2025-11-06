using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Order
{
    public class OrderItemDto
    {
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }


    }
}