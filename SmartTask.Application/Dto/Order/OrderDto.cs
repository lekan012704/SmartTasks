using SmartTask.Application.Dto.Order;
using SmartTask.Application.Enums;
using System;
using System.Collections.Generic;

namespace SmartTask.Application.Features.Orders.Queries
{
  
    public class OrderDto
    {
        public Guid Id { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? DeliveryAddress { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } 
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalDue { get; set; }
        public string? TrackingNumber { get; set; }
        public string? LogisticsPartner { get; set; }
        public string? ManualRiderName { get; set; }
        public string? ManualTrackingInfo { get; set; }
    }
}