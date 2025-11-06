using MediatR;
using SmartTask.Application.Command;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTask.Application.Users.Handlers
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Response<string>>
    {
        private readonly IEntityManagerAsync _entityManager;

        public UpdateUserCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }

        public async Task<Response<string>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
         
            return await _entityManager.UpdateUserAsync(request.UserId, request.UpdateUser);
        }
    }
}

