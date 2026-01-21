using MediatR;
using SmartTask.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTask.Application.Features.Orders.Commands
{
    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Unit>
    {
        private readonly IEntityManagerAsync _entityManager;

        public DeleteOrderCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }

        public async Task<Unit> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            return await _entityManager.DeleteOrderAsync(request);
        }
    }
}