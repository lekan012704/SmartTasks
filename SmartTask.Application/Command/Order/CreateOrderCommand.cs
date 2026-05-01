using MediatR;
using SmartTask.Application.Dto.Order;
using System.Collections.Generic;

namespace SmartTask.Application.Features.Orders.Commands
{

    public class CreateOrderCommand :IRequest<Guid>
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? WhatsAppNumber { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public decimal DeliveryFee { get; set; }
        public string? DriverName { get; set; } = string.Empty;
        public string? DriverPhone { get; set; } = string.Empty;
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }
}