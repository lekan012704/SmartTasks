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
    internal class GetOvrdueTaskQueryHandler :IRequestHandler<GetOverdueTaskQuery, Response<List<OverdueTaskStatsDto>>>
    {
        private readonly IEntityManagerAsync _entityManagerAsync;
        public GetOvrdueTaskQueryHandler(IEntityManagerAsync entityManagerAsync)
        {
            _entityManagerAsync = entityManagerAsync;
        }
        public async Task<Response<List<OverdueTaskStatsDto>>> Handle(GetOverdueTaskQuery request, CancellationToken cancellationToken)
        {
            return await _entityManagerAsync.GetOverdueTasksAsync();
        }
    }
}
