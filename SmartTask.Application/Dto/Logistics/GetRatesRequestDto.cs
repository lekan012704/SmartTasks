using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Logistics
{
    public class GetRatesRequestDto
    {
        public string RecipientName { get; set; }
        public string SenderAddress { get; set; }
    }
}
