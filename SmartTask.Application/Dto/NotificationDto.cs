using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto
{
    public class NotificationDto
    {
        public Guid ComapnyId { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? Type { get; set; } 
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? TimeAgo { get; set; } 
    }
}
