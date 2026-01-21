using MediatR;
using SmartTask.Application.Dto.Paystack;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.CommandHandler.Paystack
{
    public class VerifyBankAccountCommandHandler :IRequestHandler<VerifyBankAccountCommand, Response<AccountVerificationResponseDto>>
    {
        private readonly IEntityManagerAsync _entityManager;
        public VerifyBankAccountCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<Response<AccountVerificationResponseDto>> Handle(VerifyBankAccountCommand request, CancellationToken cancellationToken)
        {
            return await _entityManager.AccountVerification(request);
        }
    }
}
