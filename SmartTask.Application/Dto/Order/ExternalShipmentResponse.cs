using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Order
{
    public class ExternalShipmentResponse
    {
        
        public string tracking_number { get; set; }
        public string delivery_partner { get; set; }
    }
}
