using MediatR;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Query;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.QueryHandler
{
    public class GetTasksByAssignedUserQueryHandler : IRequestHandler<GetTasksByAssignedUserQuery, Response<List<TaskDto>>>
    {
        private readonly IEntityManagerAsync _entityManagerAsync;
        public GetTasksByAssignedUserQueryHandler(IEntityManagerAsync entityManagerAsync)
        {
            _entityManagerAsync = entityManagerAsync;
        }
        public async Task<Response<List<TaskDto>>> Handle(GetTasksByAssignedUserQuery request, CancellationToken cancellationToken)
        {
            return await _entityManagerAsync.GetTasksByAssignedUserAsync(request);
        }
    }
}
