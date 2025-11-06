using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Project
{
    public class ProjectMemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee"; // default, but customizable
        public DateTime DateJoined { get; set; }
    }
}
