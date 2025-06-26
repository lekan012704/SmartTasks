using MediatR;
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
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Response<CreateTaskResponse>>
    {
        private readonly ITaskService _task;
        public CreateTaskCommandHandler(ITaskService taskService)
        {
            _task = taskService;
        }
        public async Task<Response<CreateTaskResponse>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            return await _task.CreateTaskAsync(request.Task);  
        }
    }
}
