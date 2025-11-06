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
    public class GetProjectByIdQueryHandler :IRequestHandler<GetProjectByIdQuery,Response<ProjectDto>>
    {
        private readonly IEntityManagerAsync _entityManager;

        public GetProjectByIdQueryHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<Response<ProjectDto>> Handle(GetProjectByIdQuery request,CancellationToken cancellationToken)
        {
            return await _entityManager.GetProjectByIdAsync(request.ProjectId);
        }
    }
}
