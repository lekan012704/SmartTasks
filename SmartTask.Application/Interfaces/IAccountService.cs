using SmartTask.Application.Command;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Interfaces
{
    public interface IAccountService
    {
        Task<Response<LoginResponse>> LoginAsync(LoginRequest request,CancellationToken cancellationToken);


    }
}
