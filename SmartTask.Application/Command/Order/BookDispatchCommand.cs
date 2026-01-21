using MediatR;
using SmartTask.Application.Features.Orders.Commands;
using System;

namespace SmartTask.Application.Command.Order
{
    public class BookDispatchCommand : IRequest<BookDispatchResponseDto>
    {
        public Guid OrderId { get; set; }
    }
}