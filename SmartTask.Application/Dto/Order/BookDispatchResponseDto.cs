using SmartTask.Application.Enums;
using System;

namespace SmartTask.Application.Features.Orders.Commands
{
    public class BookDispatchResponseDto
    {
        public Guid OrderId { get; set; }
        public string TrackingNumber { get; set; }
        public string LogisticsPartner { get; set; }
        public OrderStatus NewStatus { get; set; }
    }
}