using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Domain.Entities
{
    public class PaystackSettings
    {
        public string SecretKey { get; set; }
        public string BaseUrl { get; set; }
    }
}
