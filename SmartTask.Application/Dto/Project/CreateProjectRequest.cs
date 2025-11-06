using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Project
{
    public class CreateProjectRequest
    {
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CompanyId { get; set; }
        public string? ProjectLeadId { get; set; }
        public string? Slug { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Active";
        public string Visibility { get; set; } = "Private";
        public string? CreatedBy { get; set; }
        public List<ProjectMemberDto>? Members { get; set; }
    }
}
