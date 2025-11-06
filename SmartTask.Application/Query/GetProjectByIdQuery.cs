using MediatR;
using SmartTask.Application.Dto.Project;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Query
{
    public class GetProjectByIdQuery :IRequest<Response<ProjectDto>>
    {
        public Guid ProjectId { get; set; }
        public GetProjectByIdQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }
}
