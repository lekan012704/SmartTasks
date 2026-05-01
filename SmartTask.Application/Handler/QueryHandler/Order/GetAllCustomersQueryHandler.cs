using MediatR;
using SmartTask.Application.Dto.Order;
using SmartTask.Application.Features.Orders.Queries;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Query.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.QueryHandler.Order
{
    public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, List<CustomerDto>>
    {
        private readonly IEntityManagerAsync _entityManager;
        public GetAllCustomersQueryHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }

        public async Task<List<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        {
            return await _entityManager.GetAllCustomersAsync();
        }
    }

}
