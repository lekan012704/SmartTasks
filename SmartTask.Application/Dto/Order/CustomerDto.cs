using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Order
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Address { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
