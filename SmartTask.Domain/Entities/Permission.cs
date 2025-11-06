using SmartTask.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Domain.Entities
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<RolePermission> RolePermission { get; set; }

    }
}
