using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Project
{
    public class CreateProjectDto
    {
        public string? ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
        public required string Status { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime StartDate { get; set; } 
        public string? Visibility { get; set; }
        public string? CreatedBy { get; set; }  
        public List<string>? MemberIds { get; set; }
    }
}
