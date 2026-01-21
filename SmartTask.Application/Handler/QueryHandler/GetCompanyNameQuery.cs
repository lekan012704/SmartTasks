using MediatR;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.QueryHandler
{
    public class GetCompanyNameQueryHandler :IRequestHandler<GetCompanyNameQuery, string>
    {
        private readonly IEntityManagerAsync _entityManager;

        public GetCompanyNameQueryHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<string> Handle(GetCompanyNameQuery request, CancellationToken cancellationToken)
        {
            return await _entityManager.GetCompanyNameAsync();
        }
    }
}
