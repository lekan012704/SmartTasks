using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTask.Application.Features.Orders.Queries
{
    public class DashboardStatsDto
    {
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalSalesMonth { get; set; }
        public int OrdersToFulfill { get; set; }
        public int PendingPayment { get; set; }
        public int TotalOrdersMonth { get; set; }
    }
}