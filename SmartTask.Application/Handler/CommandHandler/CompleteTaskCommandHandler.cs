using MediatR;
using Microsoft.Extensions.Logging;
using SmartTask.Application.Command.Task;
using SmartTask.Application.Constants;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.CommandHandler
{
    public class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, Response<bool>>
    {
        private readonly IEntityManagerAsync _entityMnageerAsync;
        public CompleteTaskCommandHandler(
            IEntityManagerAsync entityMnageerAsync)
        {
            _entityMnageerAsync = entityMnageerAsync;
        }

        public async Task<Response<bool>> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
        {
           return await _entityMnageerAsync.CompleteTaskAsync(request.TaskId);
        }

    }
}
