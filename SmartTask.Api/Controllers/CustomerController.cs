using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Api.Filter;
using SmartTask.Application.Command.Customer;
using SmartTask.Application.Dto.Customer;
using SmartTask.Application.Features.Orders.Commands;
using SmartTask.Application.Features.Orders.Queries;
using SmartTask.Application.Query.Customer;
using SmartTask.Application.Query.Order;
using SmartTask.Domain.Constants;

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
        [HasPermission(Permissions.Customers.View)]
        [HttpGet("get-customers")]
        public async Task<IActionResult> GetCustomers([FromQuery] GetAllCustomersQuery query)
        {
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        [HasPermission(Permissions.Customers.Create)]
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
        [HasPermission(Permissions.Customers.View)]
        [HttpGet("get-customer-by-id")]
        public async Task<IActionResult> GetCustomerById([FromQuery] GetCustomerByIdQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HasPermission(Permissions.Customers.Edit)]
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomer dto)
        {
            var command = new UpdateCustomerCommand(dto, id);
            await _mediator.Send(command);
            return NoContent();
        }
        [HasPermission(Permissions.Customers.Delete)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {

            var command = new DeleteCustomerCommand { CustomerId = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
