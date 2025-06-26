using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Task
{
    public class TaskStatusUpdatedDto
    {
        public Guid TaskId { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
