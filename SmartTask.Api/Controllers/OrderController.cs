using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Application.Command;
using SmartTask.Application.Command.Order;
using SmartTask.Application.Features.Orders.Commands;
using SmartTask.Application.Features.Orders.Queries;
using SmartTask.Application.Query;
using SmartTask.Application.Query.Order;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
           
            var newOrderId = await _mediator.Send(request);
            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = newOrderId }, 
                new { id = newOrderId } 
            );
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
        [HttpGet("get-orders")]
        public async Task<IActionResult> GetOrders([FromQuery] GetAllOrdersQuery query)
        {
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        
        [HttpGet("get-order-by-id")]
        public async Task<IActionResult> GetOrderById([FromQuery]GetOrderByIdQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
        {
            var command = new UpdateOrderStatusCommand
            {
                OrderId = id,
                NewStatus = dto.NewStatus
            };
            await _mediator.Send(command);
            return NoContent();
        }
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var query = new GetDashboardStatsQuery();
            var stats = await _mediator.Send(query);
            return Ok(stats);
        }
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
          
            var command = new DeleteOrderCommand { OrderId = id };
            await _mediator.Send(command);
            return NoContent();
        }
        [HttpPost("{id:guid}/book-dispatch")]
        public async Task<IActionResult> BookDispatch(Guid id)
        {
          
            var command = new BookDispatchCommand { OrderId = id };
            var dispatchDetails = await _mediator.Send(command);
            return Ok(dispatchDetails);
        }
    }
}
