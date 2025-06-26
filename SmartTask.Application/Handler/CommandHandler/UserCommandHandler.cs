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
    public class UserCommandHandler : IRequestHandler<UserCommand, Response<UserResponseDto>>
    {
        private readonly IEntityManagerAsync _entityManagerAsync;
        public UserCommandHandler(IEntityManagerAsync entityManagerAsync)
        {
            _entityManagerAsync = entityManagerAsync;
        }
        public async Task<Response<UserResponseDto>> Handle(UserCommand request, CancellationToken cancellationToken)
        {
            return await _entityManagerAsync.RegisterUserAsync(request.UserRequest);
        }
    }
}
