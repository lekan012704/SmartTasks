using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Application.Command.Customer;
using SmartTask.Application.Dto.Customer;
using SmartTask.Application.Features.Orders.Commands;
using SmartTask.Application.Features.Orders.Queries;
using SmartTask.Application.Query.Customer;
using SmartTask.Application.Query.Order;

namespace SmartTask.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("get-customers")]
        public async Task<IActionResult> GetCustomers([FromQuery] GetAllCustomersQuery query)
        {
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        [HttpPost("create-customer")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand request)
        {

            var newCustomerId = await _mediator.Send(request);
            return CreatedAtAction(
                nameof(GetCustomerById),
                new { id = newCustomerId },
                new { id = newCustomerId }
            );
        }
        [HttpGet("get-customer-by-id")]
        public async Task<IActionResult> GetCustomerById([FromQuery] GetCustomerByIdQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomer dto)
        {
            var command = new UpdateCustomerCommand(dto, id);
            await _mediator.Send(command);
            return NoContent();
        }
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCuatomer(Guid id)
        {

            var command = new DeleteCustomerCommand { CustomerId = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
