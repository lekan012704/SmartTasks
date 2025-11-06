using MediatR;
using SmartTask.Application.Dto;
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
    public class GetCompanyTypeQueryHandler :IRequestHandler<GetCompanyTypesQuery, Response<List<CompanyTypeDto>>>
    {
        private readonly IEntityManagerAsync _entity;
        public GetCompanyTypeQueryHandler(IEntityManagerAsync entity)
        {
            _entity = entity;
        }
        public async Task<Response<List<CompanyTypeDto>>> Handle(GetCompanyTypesQuery request, CancellationToken cancellationToken)
        {
          return await _entity.GetAllCompanyTypeAsync();
        }
    }
}
