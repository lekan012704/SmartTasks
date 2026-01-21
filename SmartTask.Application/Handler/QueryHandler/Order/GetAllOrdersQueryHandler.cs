using MediatR;
using SmartTask.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTask.Application.Features.Orders.Queries
{
    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, List<OrderSummaryDto>>
    {
      private readonly IEntityManagerAsync _entityManager;

        public GetAllOrdersQueryHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }

        public async Task<List<OrderSummaryDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            return await _entityManager.GetAllOrderAsync(request);
            }
    }
}