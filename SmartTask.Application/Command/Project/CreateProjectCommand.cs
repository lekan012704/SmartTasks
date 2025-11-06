using MediatR;
using SmartTask.Application.Dto.Project;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command.Project
{
    public class CreateProjectCommand :IRequest<Response<CreateProjectResponse>>
    {
        public CreateProjectRequest Request { get; set; }
        public CreateProjectCommand(CreateProjectRequest request)
        {
            Request = request;
        }
    }
}
