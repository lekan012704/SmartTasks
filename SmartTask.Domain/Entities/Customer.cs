using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Address { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
