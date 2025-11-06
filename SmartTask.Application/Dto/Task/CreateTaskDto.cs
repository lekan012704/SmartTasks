using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Task
{
    public class CreateTaskDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime DueDate { get; set; }
        public required string Priority { get; set; } // e.g., Low, Medium, High
        public required string Status { get; set; }// e.g., Not Started, In Progress, Completed
        public bool IsActive { get; set; } = true; // Default to active


    }
}
