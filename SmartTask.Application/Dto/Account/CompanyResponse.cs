using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Account
{
    public class CompanyResponse
    {
        public Guid CompanyId { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
    }
}
