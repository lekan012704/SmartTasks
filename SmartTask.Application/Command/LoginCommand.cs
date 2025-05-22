using MediatR;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command
{
    public class LoginCommand : IRequest<Response<LoginResponse>>
    {
        public LoginRequest LoginRequest { get; set; }

        public LoginCommand(LoginRequest request)
        {
            LoginRequest = request;
        }
    }
}
