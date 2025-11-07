using System;
using System.Collections.Generic;

namespace SmartTask.Application.Features.Orders.Commands
{
    public class FulfillBatchManuallyDto
    {
        public List<Guid> OrderIds { get; set; }
        public string? ManualRiderName { get; set; }
        public string? ManualTrackingInfo { get; set; }
    }
}