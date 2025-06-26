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
    public class GetTasksCompletedPerWeekQuery : IRequest<Response<List<TaskCompletionStatus>>>
    {
    }

}
