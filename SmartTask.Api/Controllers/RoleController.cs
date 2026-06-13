using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Api.Filter;
using SmartTask.Application.Command;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Query;
using SmartTask.Domain.Constants;

namespace SmartTask.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HasPermission(Permissions.Role.Create)]
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleModel request)
        {
            // Inject CompanyId from JWT so roles are scoped to the company
            request.CompanyId = User.FindFirst("CompanyId")?.Value;
            var result = await _mediator.Send(new CreateRoleCommand(request));
            return Ok(result);
        }

        [HasPermission(Permissions.Role.View)]
        [HttpGet("get/roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await _mediator.Send(new GetAllRolesQuery());
            return Ok(result);
        }

        [HasPermission(Permissions.Role.View)]
        [HttpGet("get/role/id")]
        public async Task<IActionResult> GetRoleById([FromQuery] string id)
        {
            var result = await _mediator.Send(new GetRoleByIdQuery(id));
            return Ok(result);
        }

        [HasPermission(Permissions.User.AssignRole)]
        [HttpPost("assign/role")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleCommand request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HasPermission(Permissions.Role.Edit)]
        [HttpPut("update/role")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleCommand request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HasPermission(Permissions.Role.Delete)]
        [HttpDelete("delete/role")]
        public async Task<IActionResult> DeleteRole([FromQuery] string roleId)
        {
            var result = await _mediator.Send(new DeleteRoleCommand(roleId));
            return Ok(result);
        }

        [HasPermission(Permissions.User.AssignRole)]
        [HttpPost("remove/role")]
        public async Task<IActionResult> RemoveRoleFromUser([FromBody] RemoveUserRoleCommand request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HasPermission(Permissions.Role.AssignPermissions)]
        [HttpPost("role/add-claims")]
        public async Task<IActionResult> AddClaimsToRole([FromBody] AddClaimsToRoleCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HasPermission(Permissions.Role.View)]
        [HttpGet("GetAllUsersInRole")]
        public async Task<IActionResult> GetUsersInRole([FromQuery] string roleId)
        {
            var result = await _mediator.Send(new GetUsersInRoleQuery(roleId));
            return Ok(result);
        }

        [HasPermission(Permissions.User.AssignRole)]
        [HttpPost("assign-to-role")]
        public async Task<IActionResult> AssignPermissionsToRole([FromBody] PermissionDto request)
        {
            var result = await _mediator.Send(new AssignPermissionsToRoleCommand(request));
            return Ok();
        }

        [HasPermission(Permissions.User.AssignRole)]
        [HttpPost("assign-permission-user")]
        public async Task<IActionResult> AssignPermissionsToUser([FromBody] AssignUserPermissionsDto request)
        {
            var result = await _mediator.Send(new AssignPermissionUserCommand(request));
            return Ok();
        }

        [HasPermission(Permissions.User.AssignRole)]
        [HttpGet("get-all-permission")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var response = await _mediator.Send(new GetAllPermissionsQuery());
            return Ok(response);
        }
    }
}