using MediatR;
using SmartTask.Application.Command;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.CommandHandler
{
    public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, Response<string>>
    {
        private readonly IEntityManagerAsync _entityManager;

        public ActivateUserCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }

        public async Task<Response<string>> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
        {
            return await _entityManager.ActivateUserAsync(request.UserId);
        }
    }
}
