using SmartTask.Application.Enums;
using SmartTask.Domain.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTask.Domain.Entities
{
    public class Order
    {
        // 1. Core Info
        public Guid Id { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // 2. Customer Info
        public string CustomerName { get; set; }
        public string WhatsAppNumber { get; set; }
        public string CustomerEmail { get; set; }
        public string DeliveryAddress { get; set; }
        public string OrderItemsJson { get; set; }
        public decimal Subtotal { get; set; }
        public decimal? DeliveryFee { get; set; }
        public decimal TotalDue { get; set; }

        // --- Option A: "Book Dispatch" (API) ---
        public string? TrackingNumber { get; set; } 
        public string? LogisticsPartner { get; set; } 

        // --- Option B: "Fulfill Manually" ---
        public string? ManualRiderName { get; set; } 
        public string? ManualTrackingInfo { get; set; } 

        // 6. Relationship to the User (The Seller)
        // (Assuming your user class is ApplicationUser)
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}