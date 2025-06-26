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
    public class GetFilteredOverdueTaskQueryHandler :IRequestHandler<GetFilteredOverdueTaskQuery, Response<List<OverdueTaskStatsDto>>>
    {
        private readonly IEntityManagerAsync _entityManager;
        public GetFilteredOverdueTaskQueryHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<Response<List<OverdueTaskStatsDto>>> Handle(GetFilteredOverdueTaskQuery request, CancellationToken cancellationToken)
        {
            return await _entityManager.GetFilteredOverdueTasksAsync(request.filtered);
        }
    }
}
