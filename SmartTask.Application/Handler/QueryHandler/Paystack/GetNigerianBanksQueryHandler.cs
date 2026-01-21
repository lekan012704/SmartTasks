using MediatR;
using SmartTask.Application.Dto.Paystack;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Query.Paystack;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.QueryHandler.Paystack
{
    public class GetNigerianBanksQueryHandler :IRequestHandler<GetNigerianBanksQuery, Response<List<BankDto>>>
    {
        private readonly IEntityManagerAsync _entityManager;

        public GetNigerianBanksQueryHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<Response<List<BankDto>>> Handle(GetNigerianBanksQuery request, CancellationToken cancellationToken)
        {
            return await _entityManager.GetNigerianBanksAsync();
        }
    }
}
