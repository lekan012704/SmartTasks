using SmartTask.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Task
{
    public class WeeklyStatsFilter
    {
        public string? Week { get; set; }
        public int? MinCompletedTasks { get; set; }
        public TaskStatuses? Status { get; set; }
    }
}
