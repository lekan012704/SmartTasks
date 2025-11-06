using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Api.Filter;
using SmartTask.Application.Command.Project;
using SmartTask.Application.Command.Task;
using SmartTask.Application.Dto.Project;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Query;
using SmartTask.Domain.Constants;

namespace SmartTask.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ProjectController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HasPermission(Permissions.Project.View)]
        [HttpGet("get-project-by-id")]
        public async Task<IActionResult> GetProjectById([FromQuery] Guid projectId)
        {
            var result = await _mediator.Send(new GetProjectByIdQuery(projectId));
            return Ok(result);
        }
        [HasPermission(Permissions.Project.View)]
        [HttpGet("get-project-by-company-id")]
        public async Task<IActionResult> GetProjectBycCompanyId()
        {
            var result = await _mediator.Send(new GetProjectByComapanyIdQuery());
            return Ok(result);
        }
        [HasPermission(Permissions.Project.Create)]
        [HttpPost("create-project")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var result = await _mediator.Send(new CreateProjectCommand(request));
            return Ok(result);
        }
    }
}
