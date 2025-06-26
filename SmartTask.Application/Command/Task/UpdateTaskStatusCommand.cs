using MediatR;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command.Task
{
    public class UpdateTaskCommand : IRequest<Response<TaskDto>>
    {
     public TaskUpdate TaskUpdate { get; set; }
    public UpdateTaskCommand(TaskUpdate taskUpdate)
        {
            TaskUpdate = taskUpdate ?? throw new ArgumentNullException(nameof(taskUpdate));
        }
    }

}
