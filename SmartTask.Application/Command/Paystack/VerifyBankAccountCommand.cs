using MediatR;
using SmartTask.Application.Dto.Paystack;
using SmartTask.Application.Wrappers;
using System.Collections.Generic;


public class VerifyBankAccountCommand : IRequest<Response<AccountVerificationResponseDto>>
{

    public string AccountNumber { get; set; }
    public string BankCode { get; set; }
}