using MediatR;
using SmartTask.Application.Dto.Logistics;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Query.Logistics
{
    public class GetLogisticsRatesQuery : IRequest<Response<List<ShipbubbleRateOption>>>
    {
        public Guid OrderId { get; set; }
    }
}
    