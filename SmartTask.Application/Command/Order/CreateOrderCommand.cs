using MediatR;
using SmartTask.Application.Dto.Order;
using System.Collections.Generic;

namespace SmartTask.Application.Features.Orders.Commands
{
   
    public class CreateOrderCommand : IRequest<Guid> 
    {
        // 1. Customer Info
        public string CustomerName { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? DeliveryAddress { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();

        public decimal DeliveryFee { get; set; }

    }
}