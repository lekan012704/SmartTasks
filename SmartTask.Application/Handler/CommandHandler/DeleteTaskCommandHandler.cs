using MediatR;
using SmartTask.Application.Command.Task;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.CommandHandler
{
    public class DeleteTaskCommandHandler :IRequestHandler<DeleteTaskCommand, Response<bool>>
    {
        private readonly ITaskService _task;
        public DeleteTaskCommandHandler(ITaskService taskService)
        {
            _task = taskService;
        }
        public async Task<Response<bool>> Handle(DeleteTaskCommand request,CancellationToken cancellationToken)
        {
            return await _task.DeleteTaskAsync(request);
        }
    }
}
