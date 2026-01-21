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
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Response<string>>
    {
        private readonly IEntityManagerAsync _entityManager;

        public ResetPasswordCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
    
    public async Task<Response<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            return await _entityManager.ResetPasswordAsync(request);
        }
    }
}
