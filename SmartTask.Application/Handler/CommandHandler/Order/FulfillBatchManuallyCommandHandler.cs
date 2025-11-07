using MediatR;
using SmartTask.Application.Command.Order;
using SmartTask.Application.Enums;
using SmartTask.Application.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTask.Application.Features.Orders.Commands
{
    public class FulfillBatchManuallyCommandHandler : IRequestHandler<FulfillBatchManuallyCommand, Unit>
    {
        private readonly IEntityManagerAsync _entityManager;

        public FulfillBatchManuallyCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }

        public async Task<Unit> Handle(FulfillBatchManuallyCommand request, CancellationToken cancellationToken)
        {
            return await _entityManager.FulfillBatchManuallyAsync(request);
        }
    }
}