using MediatR;
using System.Collections.Generic;

namespace SmartTask.Application.Features.Orders.Queries
{
    public class GetAllOrdersQuery : IRequest<List<OrderSummaryDto>>
    {
    }
}