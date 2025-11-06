using MediatR;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Query;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.QueryHandler
{
    public class GetUsersByCompanyQueryHandler : IRequestHandler<GetUsersByCompany, Response<List<UserDto>>>
    {
        private readonly IEntityManagerAsync _entityManagerAsync;
        public GetUsersByCompanyQueryHandler(IEntityManagerAsync entityManagerAsync)
        {
            _entityManagerAsync = entityManagerAsync;
        }
        public async Task<Response<List<UserDto>>> Handle(GetUsersByCompany request, CancellationToken cancellationToken)
        {
            return await _entityManagerAsync.GetUsersByCompanyAsync(request);
        }
    }
}
