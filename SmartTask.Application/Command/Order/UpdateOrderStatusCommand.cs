using MediatR;
using SmartTask.Application.Enums;
using System;

namespace SmartTask.Application.Features.Orders.Commands
{
    public class UpdateOrderStatusCommand : IRequest<Unit>
    {
        public Guid OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
    }
}