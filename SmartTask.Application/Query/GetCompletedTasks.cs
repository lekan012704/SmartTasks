using MediatR;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Wrappers;
using System.Collections.Generic;

namespace SmartTask.Application.Query.Task
{
    public class GetCompletedTasksQuery : IRequest<Response<List<CompletedTaskDto>>>
    {

    }
}
