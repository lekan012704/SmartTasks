using MediatR;
using SmartTask.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTask.Application.Features.Orders.Queries
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
    {
        private readonly IEntityManagerAsync _entityManager;

        public GetOrderByIdQueryHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }

        public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            return await _entityManager.GetOrderByIdAsync(request);
        }
    }
}