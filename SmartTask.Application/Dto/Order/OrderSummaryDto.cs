using SmartTask.Application.Enums;
using System;

namespace SmartTask.Application.Features.Orders.Queries
{
    
    public class OrderSummaryDto
    {
        public Guid Id { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalDue { get; set; }
    }
}