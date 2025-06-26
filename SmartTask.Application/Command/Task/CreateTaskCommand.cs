using MediatR;
using MediatR.NotificationPublishers;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command.Task
{
    public class CreateTaskCommand :IRequest<Response<CreateTaskResponse>>
    {
        public CreateTaskDto Task { get; set; }
        public CreateTaskCommand(CreateTaskDto createTask)
        {
            Task = createTask;
        }
    }
}
