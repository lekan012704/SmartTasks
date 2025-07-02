using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Task
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AssignedUserId { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; } // e.g., Low, Medium, High
        public string Status { get; set; }// e.g., Not Started, In Progress, Completed
        public string CreatedBy { get; set; } // User ID of the creator
        public bool IsActive { get; set; } = true; // Default to active
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default to current time
    }
}
