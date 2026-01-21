using MediatR;
using SmartTask.Application.Command.Order;
using SmartTask.Application.Enums;
using SmartTask.Application.Interfaces;
using SmartTask.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTask.Application.Features.Orders.Commands
{
    public class BookDispatchCommandHandler : IRequestHandler<BookDispatchCommand, BookDispatchResponseDto>
    {
        private readonly IEntityManagerAsync _entityManagerAsync;

        public BookDispatchCommandHandler(IEntityManagerAsync entityManagerAsync)
        {
            _entityManagerAsync = entityManagerAsync;
        }

        public async Task<BookDispatchResponseDto> Handle(BookDispatchCommand request, CancellationToken cancellationToken)
        {
            return await _entityManagerAsync.DispatchOrderAsync(request.OrderId);
        }
    }
}