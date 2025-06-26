using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid TaskId { get; set; }
        public string Action { get; set; } 
        public string PerformedBy { get; set; }
        public DateTime PerformedAt { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }
}
