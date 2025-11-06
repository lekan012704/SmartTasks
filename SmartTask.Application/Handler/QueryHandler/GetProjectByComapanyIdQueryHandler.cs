using MediatR;
using SmartTask.Application.Dto.Project;
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
    public class GetProjectByComapanyIdQueryHandler :IRequestHandler<GetProjectByComapanyIdQuery, Response<List<ProjectDto>>>
    {
        private readonly IEntityManagerAsync _entityManger;

        public GetProjectByComapanyIdQueryHandler(IEntityManagerAsync entityManger)
        {
            _entityManger = entityManger;
        }
        public async Task<Response<List<ProjectDto>>> Handle(GetProjectByComapanyIdQuery request, CancellationToken cancellationToken)
        {
            return await _entityManger.GetProjectByCompanyIdAsync();
        }
    }
}
