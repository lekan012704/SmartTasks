using MediatR;
using SmartTask.Application.Command.Project;
using SmartTask.Application.Dto.Project;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.CommandHandler
{
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Response<CreateProjectResponse>>
    {
        private readonly IEntityManagerAsync _entityManagerAsync;

        public CreateProjectCommandHandler(IEntityManagerAsync entityManagerAsync)
        {
            _entityManagerAsync = entityManagerAsync;
        }
    
    public async Task<Response<CreateProjectResponse>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            return await _entityManagerAsync.CreateProjectAsync(request.Request);
        }
    }
}
