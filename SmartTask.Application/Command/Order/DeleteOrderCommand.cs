using MediatR;
using System;

namespace SmartTask.Application.Features.Orders.Commands
{
    public class DeleteOrderCommand : IRequest<Unit>
    {
        public Guid OrderId { get; set; }
    }
}