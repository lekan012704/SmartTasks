using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Application.Query.Paystack;

namespace SmartTask.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("get-banks")]
        public async Task<IActionResult> GetBanks()
        {
            var result = await _mediator.Send(new GetNigerianBanksQuery());
            return Ok(result);
        }
        [HttpPost("verify-account")]
        public async Task<IActionResult> VerifyAccount([FromBody] VerifyBankAccountCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            return BadRequest(result);
        }
    }
}
