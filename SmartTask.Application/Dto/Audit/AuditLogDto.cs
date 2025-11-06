using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Audit
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public Guid TaskId { get; set; }
        public string PerformedBy { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime PerformedAt { get; set; }
    }

}
