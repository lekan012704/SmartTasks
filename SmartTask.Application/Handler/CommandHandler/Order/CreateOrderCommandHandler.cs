using MediatR;
using Newtonsoft.Json; // For serializing the OrderItems
using SmartTask.Application.Interfaces; 
using SmartTask.Domain.Entities;
using SmartTask.Domain.Enums;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTask.Application.Features.Orders.Commands
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
       private readonly IEntityManagerAsync _entityManager;

        public CreateOrderCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            return await _entityManager.CreateOrderAsync(request, cancellationToken);
        }
    }
}