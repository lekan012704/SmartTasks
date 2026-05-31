//using MediatR;
//using SmartTask.Application.Dto.Logistics;
//using SmartTask.Application.Interfaces;
//using SmartTask.Application.Query.Logistics;
//using SmartTask.Application.Wrappers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SmartTask.Application.Handler.QueryHandler.Logistics
//{
//    public class GetLogisticsRatesQueryHandler :IRequestHandler<GetLogisticsRatesQuery, Response<List<ShipbubbleRateOption>>>
//    {
//        private readonly IEntityManagerAsync _entityManager;

//        public GetLogisticsRatesQueryHandler(IEntityManagerAsync entityManager)
//        {
//            _entityManager = entityManager;
//        }
//        public async Task<Response<List<ShipbubbleRateOption>>> Handle(GetLogisticsRatesQuery request, CancellationToken cancellationToken)
//        {
//            return await _entityManager.GetRates(request.OrderId);
//        }
//    }
//}
