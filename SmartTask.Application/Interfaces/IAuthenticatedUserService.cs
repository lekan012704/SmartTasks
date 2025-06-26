using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Interfaces
{
    public interface IAuthenticatedUserService
    {
        string UserId { get; }
        string Email { get; }
        string UserName { get; }
        string CompanyId { get; }   
        string CompanyName { get; }
        string Role { get; }
        string RoleName { get; }
        
    }
}
