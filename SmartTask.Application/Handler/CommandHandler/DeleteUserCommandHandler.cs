using MediatR;
using SmartTask.Application.Command;
using SmartTask.Application.Interfaces; 
using SmartTask.Application.Wrappers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTask.Application.Users.Handlers
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Response<string>>
    {
        private readonly IEntityManagerAsync _entityManager;

        public DeleteUserCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }

        public async Task<Response<string>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {   
            return await _entityManager.DeleteUserAsync(request.UserId);
        }
    }
}

