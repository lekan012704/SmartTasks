using MediatR;
using SmartTask.Application.Dto.Order;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Query.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.QueryHandler.Customer
{
    public class GetCustomerByIdQueryHander : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
    {
        private readonly IEntityManagerAsync _entityManager;
        public GetCustomerByIdQueryHander(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            return await _entityManager.GetCustomerByIdAsync(request);
        }
    }
}
