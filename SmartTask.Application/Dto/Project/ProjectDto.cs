using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Project
{
    public class ProjectDto
    {
        public Guid ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? Status { get; set; }
        public string? Visibility { get; set; }
        public Guid CompanyId { get; set; }
        public string? ProjectLeadId { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int TotalTasks { get; set; }
        public int OpenTasks { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public int TaskCount { get; set; }
        public int SprintCount { get; set; } 
        public List<ProjectMemberDto> Members { get; set; } = new List<ProjectMemberDto>();
    }
}
