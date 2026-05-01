using MediatR;
using SmartTask.Application.Dto.Order;
using SmartTask.Application.Features.Orders.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Query.Order
{
    public class GetAllCustomersQuery : IRequest<List<CustomerDto>>
    {

    }
}
