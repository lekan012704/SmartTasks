using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Api.Filter;
using SmartTask.Application.Command.Task;
using SmartTask.Application.Constants;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Query;
using SmartTask.Application.Query.Task;
using SmartTask.Domain.Constants;

namespace SmartTask.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]

    public class TaskController : ControllerBase
    {
        private readonly IMediator _mediator;
            
        public TaskController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HasPermission(Permissions.Task.Create)]
        [Authorize]
        [HttpPost("create-task")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto request)
        {
            var result = await _mediator.Send(new CreateTaskCommand(request));
            return Ok(result);
        }
        [HasPermission(Permissions.Task.View)]
        [HttpGet("get-task-by-company-id")]
        public async Task<IActionResult> GetTaskByCompanyId([FromQuery] GetTaskByComapanyIdQuery request)
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
        [HasPermission(Permissions.Task.Edit)]
        [HttpPut("update-task")]
        public async Task<IActionResult> UpdateTaskStatus([FromBody] TaskUpdate request)
        {
            var result = await _mediator.Send(new UpdateTaskCommand(request));
            return Ok(result);
        }
        [HasPermission(Permissions.Task.Delete)]
        [HttpDelete("delete-task")]
        public async Task<IActionResult> DeleteTask([FromQuery] Guid Id)
        {
            var command = new DeleteTaskCommand(Id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        [AllowAnonymous]
        [HttpGet("hello")]
        public IActionResult SayHello()
        {
            return Ok("Hello from Task Controller!");
        }
        [HasPermission(Permissions.Task.View)]
        [HttpGet("completed-tasks")]
        public async Task<IActionResult> GetCompletedTasks()
        {
            var result = await _mediator.Send(new GetCompletedTasksQuery());
            return Ok(result);
        }
        [HttpPut("{taskId}/complete")]
        [HasPermission(Permissions.Task.Edit)]
        public async Task<IActionResult> CompleteTask(Guid taskId)
        {
             var command = new CompleteTaskCommand(taskId);
            var result =await _mediator.Send(command);
            return Ok(result);
        }
        [HttpGet("{taskId}")]
        [HasPermission(Permissions.Task.View)]
        public async Task<IActionResult> GetTaskById(Guid taskId)
        {
            if (taskId == Guid.Empty)
            {
                // Return a failure response consistent with your ApplicationConstants
                return BadRequest(ApplicationConstants.FailureMessage<TaskDto>(null, "Task ID must be provided."));
            }

            var query = new GetTaskByIdQuery(taskId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }


        }
    }