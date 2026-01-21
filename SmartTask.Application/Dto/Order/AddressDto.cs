using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Order
{
    public class AddressDto
    {
        public string phone { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
    }

    public class PackageDto
    {
        public decimal Weight { get; set; }
        public decimal Amount { get; set; }
    }

    public class RateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; } 
    }

}
