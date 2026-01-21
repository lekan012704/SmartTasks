namespace SmartTask.Application.Dto.Order
{
    public class ShipmentResultDto
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public string CourierName { get; set; } = string.Empty;
        public string CourierId { get; set; } = string.Empty;
        public string ServiceCode { get; set; } = string.Empty;
        public string LabelUrl { get; set; } = string.Empty;
        public string DeliveryETA { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
