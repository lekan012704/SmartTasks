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
    public class ForgotPasswordCommandHandler :IRequestHandler<ForgotPasswordCommand, Response<string>>
    {
        private readonly IEntityManagerAsync _entityManager;

        public ForgotPasswordCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<Response<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
           return await _entityManager.ForgotPasswordAsync(request);
            }
    }
}
