using MediatR;
using SmartTask.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTask.Application.Features.Orders.Commands
{
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Unit>
    {
       private readonly IEntityManagerAsync _entityManager;

        public UpdateOrderStatusCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }

        public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            return await _entityManager.UpdateStatusAsync(request);
        }
    }
}