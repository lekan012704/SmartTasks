using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Task
{
    public class TaskUpdate
    {
            public Guid TaskId { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? Status { get; set; }  // enum as string
            public string? Priority { get; set; } // enum as string
            public string AssignedUserId { get; set; }
            public DateTime? DueDate { get; set; }
            public string UpdatedBy { get; set; }
        }

    }

