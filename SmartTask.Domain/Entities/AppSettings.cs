using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Domain.Entities
{
    public class AppSettings
    {
        public string ApplicationName { get; set; }
        public string DefaultSuperAdminEmail { get; set; }
        public string DefaultSuperAdminPassword { get; set; }
        public bool AutoConfirmEmail { get; set; } = true;
        public string FrontendUrl { get; set; }
    }
}
