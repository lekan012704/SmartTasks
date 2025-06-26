using SmartTask.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Task
{
    public class TaskCompletionStatus
    {
      
            public string Week { get; set; }
            public int CompletedTasks { get; set; }
            public double AvgCompletionDays { get; set; }
            public int OverdueCount { get; set; }
            public int HighPriorityCompleted { get; set; }
            public int MediumPriorityCompleted { get; set; }
            public int LowPriorityCompleted { get; set; }
            public TaskStatuses Status { get; set; }
        }
    }


