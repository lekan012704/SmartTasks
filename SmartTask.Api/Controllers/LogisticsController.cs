using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Application.Query.Logistics;

namespace SmartTask.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class LogisticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LogisticsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("get-rates")]
        public async Task<IActionResult> GetRates([FromQuery] GetLogisticsRatesQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
