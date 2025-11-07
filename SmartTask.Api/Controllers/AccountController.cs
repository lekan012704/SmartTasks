using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Api.Filter;
using SmartTask.Application.Command;
using SmartTask.Application.Constants;
using SmartTask.Application.Dto;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Query;
using SmartTask.Domain.Constants;

namespace SmartTask.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class AccountController : ControllerBase
    {   
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("get-all-company-types")]
        public async Task<IActionResult> GetAllCompanyTypes()
        {
            var result = await _mediator.Send(new GetCompanyTypesQuery());
            return Ok(result);
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
        [HasPermission(Permissions.User.Edit)]
        [HttpPut("update-user/{id}")]
        public async Task<IActionResult> UpdateUser(string id,[FromBody] UpdateUserRequestDto requestDto)
        {
            var command = new UpdateUserCommand(requestDto,id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        [HasPermission(Permissions.Role.AssignPermissions)]
        [HttpPost("add-permission")]
        public async Task<IActionResult> AddPermission([FromBody] PermissionDto request)
        {
            var result = await _mediator.Send(new AddPermissionCommand(request));
            return Ok(result);
        }
        //[HasPermission(Permissions.Report.View)]
        //[HttpGet("get-completed-task")]
        //public async Task<IActionResult> GetCompletedTask()
        //{
        //    var result = await _mediator.Send(new GetTasksCompletedPerWeekQuery());
        //    return Ok(result);
        //}
        //[HasPermission(Permissions.Report.View)]
        //[HttpGet("get-completed-tasks-filtered")]
        //public async Task<IActionResult> GetCompletedTaskFiltered([FromQuery] WeeklyStatsFilter request)
        //{
        //    var result = await _mediator.Send(new GetFilteredTasksCompletedPerWeekQuery(request));
        //    return Ok(result);
        //}
        //[HasPermission(Permissions.Report.View)]
        //[HttpGet("get-over - due-task-by-status")]
        //public async Task<IActionResult> GetOverDueTaskByStatus()
        //{
        //    var result = await _mediator.Send(new GetOverdueTaskQuery());
        //    return Ok(result);
        //}
        //[HasPermission(Permissions.Report.View)]
        //[HttpGet("get-over-due-task-by-status-filtered")]
        //public async Task<IActionResult> GetOverDueTaskByStatusFiltered([FromQuery] FilteredOverdueTask request)
        //{
        //    var result = await _mediator.Send(new GetFilteredOverdueTaskQuery(request));
        //    return Ok(result);
        //}
       
        //[HasPermission(Permissions.Audit.View)]
        //[HttpGet("get-audit-log")]
        //public async Task<IActionResult> GetAuditLog()
        //{
        //    var result = await _mediator.Send(new GetAuditQuery());
        //    return Ok(result);
        //}
        //[HasPermission(Permissions.Audit.View)]
        //[HttpGet("get-audit-log-filtered")]
        //public async Task<IActionResult> GetAuditLogFiltered([FromQuery] FilteredAuditLog request)
        //{
        //    var result = await _mediator.Send(new GetFilteredAuditLog(request));
        //    return Ok(result);
        //}
        [HasPermission(Permissions.User.Activate)]
        [HttpPost("activate-user/{id}")] 
        public async Task<IActionResult> ActivateUser(string id)
        {
            var command = new ActivateUserCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
            
        }
        [HasPermission(Permissions.User.Deactivate)]
        [HttpPost("deactivate-user/{id}")]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var command = new DectivateUserCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);

        }
        [HasPermission(Permissions.User.Delete)]
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var command = new DeleteUserCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);

        }
        [HttpGet("get-users-by-company")]
        [HasPermission(Permissions.User.View)]
        public async Task<IActionResult> GetUsersByCompany([FromQuery] GetUsersByCompany request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
        [Authorize]
        [HttpGet("auth-test")]
        public IActionResult TestAuth()
        {
            return Ok("You are authenticated");
        }
    }
}
