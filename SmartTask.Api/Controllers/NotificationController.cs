using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Application.Query;

namespace SmartTask.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class NotoficationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotoficationController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetUserNotificationsQuery { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
