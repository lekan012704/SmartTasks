using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Application.Command.Order;
using SmartTask.Application.Features.Orders.Commands;
using SmartTask.Application.Query;

namespace SmartTask.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
        [HttpPost("fulfill-batch-manually")]
        public async Task<IActionResult> FulfillBatchManually([FromBody] FulfillBatchManuallyDto dto)
        {
            
            var command = new FulfillBatchManuallyCommand
            {
                OrderIds = dto.OrderIds,
                ManualRiderName = dto.ManualRiderName,
                ManualTrackingInfo = dto.ManualTrackingInfo
            };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
