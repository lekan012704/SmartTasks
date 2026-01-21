using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Interfaces
{
    public interface  INotificationService
    {
        Task SendNotificationAsync(Guid userId, string title, string message, string type);
    }
}
