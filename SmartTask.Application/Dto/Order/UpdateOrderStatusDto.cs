using SmartTask.Application.Enums;

namespace SmartTask.Application.Features.Orders.Commands
{
    public class UpdateOrderStatusDto
    {
        public OrderStatus NewStatus { get; set; }
    }
}