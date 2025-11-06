using MediatR;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command.Task
{
    public class CompleteTaskCommand : IRequest<Response<bool>> // Returns success/failure
    {
 
        public Guid TaskId { get; }

        public CompleteTaskCommand(Guid taskId)
        {
            if (taskId == Guid.Empty)
            {
                throw new ArgumentException("Task ID cannot be empty.", nameof(taskId));
            }
            TaskId = taskId;
        }
    }
}
