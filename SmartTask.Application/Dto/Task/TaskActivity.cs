using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Task
{
    public class TaskActivity
    {
        public Guid TaskId { get; set; }
        public string Action { get; set; } 
        public string PerformedBy { get; set; } 
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Description { get; set; }
    }
}
