using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Api.Filter;
using SmartTask.Application.Command;
using SmartTask.Application.Command.Task;
using SmartTask.Application.Constants;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Dto.Audit;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Query;
using SmartTask.Domain.Constants;

namespace SmartTask.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("login")]

        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _mediator.Send(new LoginCommand(request));
            return Ok(result);
        }
        [HttpPost("register-company")]
        public async Task<IActionResult> Register([FromBody] CompanyRequest request)
        {
            var result = await _mediator.Send(new RegisterCompanyCommand(request));
            return Ok(result);
        }
        [HasPermission(Permissions.User.Create)]
        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRequestDto request)
        {
            var result = await _mediator.Send(new UserCommand(request));
            return Ok(result);
        }
        [HasPermission(Permissions.Task.Create)]
        [HttpPost("create-task")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto request)
        {
            var result = await _mediator.Send(new CreateTaskCommand(request));
            return Ok(result);
        }
        [HasPermission(Permissions.Task.View)]
        [HttpGet("get-task-by-company-id")]
        public async Task<IActionResult> GetTaskByCompanyId([FromQuery] GetTaskByIdQuery request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
        [HasPermission(Permissions.Task.Assign)]
        [HttpGet("get-task-by-assigned-user-email")]
        public async Task<IActionResult> GetTaskByAssignedUserEmail([FromQuery] GetTasksByAssignedUserQuery request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
        [HasPermission(Permissions.Role.AssignPermissions)]
        [HttpPost("add-permission")]
        public async Task<IActionResult> AddPermission([FromBody] PermissionDto request)
        {
            var result = await _mediator.Send(new AddPermissionCommand(request));
            return Ok(result);
        }
        [HasPermission(Permissions.Report.View)]
        [HttpGet("get-completed-task")]
        public async Task<IActionResult> GetCompletedTask()
        {
            var result = await _mediator.Send(new GetTasksCompletedPerWeekQuery());
            return Ok(result);
        }
        [HasPermission(Permissions.Report.View)]
        [HttpGet("get-comleted-tasks-filtered")]
        public async Task<IActionResult> GetCompletedTaskFiltered([FromQuery] WeeklyStatsFilter request)
        {
            var result = await _mediator.Send(new GetFilteredTasksCompletedPerWeekQuery(request));
            return Ok(result);
        }
        [HasPermission(Permissions.Report.View)]
        [HttpGet("get-over - due-task-by-status")]
        public async Task<IActionResult> GetOverDueTaskByStatus()
        {
            var result = await _mediator.Send(new GetOverdueTaskQuery());
            return Ok(result);
        }
        [HasPermission(Permissions.Report.View)]
        [HttpGet("get-over-due-task-by-status-filtered")]
        public async Task<IActionResult> GetOverDueTaskByStatusFiltered([FromQuery] FilteredOverdueTask request)
        {
            var result = await _mediator.Send(new GetFilteredOverdueTaskQuery(request));
            return Ok(result);
        }
        [HasPermission(Permissions.Task.Edit)]
        [HttpPut("update-task")]
        public async Task<IActionResult> UpdateTaskStatus([FromBody] TaskUpdate request)
        {
            var result = await _mediator.Send(new UpdateTaskCommand(request));
            return Ok(result);
        }
        [HasPermission(Permissions.Audit.View)]
        [HttpGet("get-audit-log")]
        public async Task<IActionResult> GetAuditLog()
        {
            var result = await _mediator.Send(new GetAuditQuery());
            return Ok(result);
        }
        [HasPermission(Permissions.Audit.View)]
        [HttpGet("get-audit-log-filtered")] 
        public async Task<IActionResult> GetAuditLogFiltered([FromQuery] FilteredAuditLog request)
        {
            var result = await _mediator.Send(new GetFilteredAuditLog(request));
            return Ok(result);
        }
        [HasPermission(Permissions.Task.Delete)]
        [HttpDelete("delete-task")]
        public async Task<IActionResult> DeleteTask([FromQuery] DeleteTaskCommand request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
    }
}
