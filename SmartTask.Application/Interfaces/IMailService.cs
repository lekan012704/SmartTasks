using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Interfaces
{
    public interface IMailService
    {
        Task SendAsync(string toEmail, string subject, string body);
    }
}
