using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Audit
{
    public class FilteredAuditLog
    {
        public Guid? TaskId { get; set; }
        public DateTime? startDate { get; set; } 
        public DateTime? endDate { get; set; } 
        public string PerformedBy { get; set; } 
       
    }
}
    