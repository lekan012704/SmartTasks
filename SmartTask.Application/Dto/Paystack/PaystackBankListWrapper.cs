using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Paystack
{
    public class PaystackBankListWrapper
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<PaystackBankData> Data { get; set; }
    }

    public class PaystackBankData
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Slug { get; set; }
    }
}
