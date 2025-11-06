using MediatR;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Query
{
    public class GetTaskByIdQuery : IRequest<Response<TaskDto>>
    {
        public Guid TaskId { get; }

        public GetTaskByIdQuery(Guid taskId)
        {
            TaskId = taskId;
        }
    }
}
