using MediatR;
using SmartTask.Application.Command;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.CommandHandler
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Response<LoginResponse>>
    {
        private readonly IAccountService _account;

        public LoginCommandHandler(IAccountService account)
        {
            _account = account;
        }

        public async Task<Response<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            return await _account.LoginAsync(request.LoginRequest,cancellationToken);
        }

    }

}
