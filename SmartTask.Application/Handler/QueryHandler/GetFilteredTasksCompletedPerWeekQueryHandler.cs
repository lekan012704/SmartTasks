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
    public class GetFilteredTasksCompletedPerWeekQueryHandler :IRequestHandler<GetFilteredTasksCompletedPerWeekQuery,Response<List<TaskCompletionStatus>>>
    {
        private readonly IEntityManagerAsync _entityManager;
        public GetFilteredTasksCompletedPerWeekQueryHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<Response<List<TaskCompletionStatus>>> Handle(GetFilteredTasksCompletedPerWeekQuery request, CancellationToken cancellationToken)
        {
            return await _entityManager.GetFilteredTasksAsync(request.Filter);
        }   
    }
}
