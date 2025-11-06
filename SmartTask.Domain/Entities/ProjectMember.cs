using SmartTask.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Domain.Entities
{
    public class ProjectMember
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public string? Role { get; set; } 
        public DateTime DateJoined { get; set; }
    }

}
