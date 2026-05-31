using SmartTask.Application.Enums;
using SmartTask.Domain.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTask.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Customer relationship
        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // Keep these directly on Order — snapshot of customer info at order time
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? WhatsAppNumber { get; set; }

        // Driver
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }

        public string DeliveryAddress { get; set; } = string.Empty;
        public string OrderItemsJson { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal? DeliveryFee { get; set; }
        public decimal TotalDue { get; private set; }
        public void RecalculateTotal()
        {
            TotalDue = Subtotal + (DeliveryFee ?? 0);
        }
        public string? TrackingNumber { get; set; }
        public string? LogisticsPartner { get; set; }
        public string? ManualRiderName { get; set; }
        public string? ManualTrackingInfo { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
    }
}