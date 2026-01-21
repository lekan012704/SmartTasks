using MediatR;
using SmartTask.Application.Enums;
using SmartTask.Application.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTask.Application.Features.Orders.Queries
{
    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
    {
        private readonly IEntityManagerAsync _entityManager;
        public GetDashboardStatsQueryHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }

        public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            return await _entityManager.GetDasboardAsync();
        }
    }
}