using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Paystack
{
    public class PaystackResolveResponse
    {
        public bool status { get; set; } 
        public string message { get; set; }
        public PaystackAccountData data { get; set; } 
    }

    public class PaystackAccountData
    {
        public string account_number { get; set; }
        public string account_name { get; set; } 
        public int bank_id { get; set; }
    }
}
