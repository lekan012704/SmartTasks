using MediatR;
using System;
using System.Collections.Generic;

namespace SmartTask.Application.Command.Order
{
    public class FulfillBatchManuallyCommand : IRequest<Unit>
    {
        public List<Guid> OrderIds { get; set; }
        public string? ManualRiderName { get; set; }
        public string? ManualTrackingInfo { get; set; }
    }
}