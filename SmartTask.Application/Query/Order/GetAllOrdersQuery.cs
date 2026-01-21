using MediatR;
using System.Collections.Generic;

namespace SmartTask.Application.Features.Orders.Queries
{
    public class GetAllOrdersQuery : IRequest<List<OrderSummaryDto>>
    {
        public string? Search { get; set; }
        public List<int>? StatusIds { get; set; }

        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}