using MediatR;
using SmartTask.Application.Command;
using SmartTask.Application.Command.Task;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.CommandHandler
{
    internal class UpdateTaskCommandHandler :IRequestHandler<UpdateTaskCommand, Response<TaskDto>>
    {
        private readonly ITaskService _task;
        public UpdateTaskCommandHandler(ITaskService taskService)
        {
            _task = taskService;
        }
        public async Task<Response<TaskDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            return await _task.UpdateTaskAsync(request.TaskUpdate);
        }
    }
}
