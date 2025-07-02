using SmartTask.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Domain.Entities
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public TaskStatuses Status { get; set; } = TaskStatuses.New;
        public TaskPriority Priority { get; set; } = TaskPriority.Low;
        public string CreatedBy { get; set; }   
        public string? AssignedUserId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public Guid CompanyId { get; set; } 
        public Company Company { get; set; }
        public bool isActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool OverdueReminderSent { get; set; } = false;

    }
}
