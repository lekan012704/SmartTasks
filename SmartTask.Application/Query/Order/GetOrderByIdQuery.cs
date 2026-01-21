using MediatR;
using System;

namespace SmartTask.Application.Features.Orders.Queries
{
  
    public class GetOrderByIdQuery : IRequest<OrderDto>
    {
        public Guid OrderId { get; set; }
    }
}