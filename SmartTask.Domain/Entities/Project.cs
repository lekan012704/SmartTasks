using SmartTask.Application.Enums;
using SmartTask.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Domain.Entities
{
    public class Project
    {
        public Guid ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public ProjectStatus Status { get; set; }
        public ProjectVisibility Visibility { get; set; }
        public Guid CompanyId { get; set; }
        public string? ProjectLeadId { get; set; }
        public virtual ApplicationUser? ProjectLead { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int TotalTasks { get; set; }
        public int OpenTasks { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
         public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
         public virtual ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
    }
}
