using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Task
{
    public class CreateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid AssignedUserId { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; } // e.g., Low, Medium, High
        public string Status { get; set; }// e.g., Not Started, In Progress, Completed
        public bool IsActive { get; set; } = true; // Default to active


    }
}
