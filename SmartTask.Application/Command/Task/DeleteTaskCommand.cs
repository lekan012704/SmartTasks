using MediatR;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command.Task
{
    public class DeleteTaskCommand : IRequest<Response<bool>>
    {
      public Guid TaskId { get; set; }
        public DeleteTaskCommand(Guid taskId)
        {
            TaskId = taskId;
        }
    }
}
