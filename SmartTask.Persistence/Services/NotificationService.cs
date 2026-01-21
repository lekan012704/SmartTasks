using Microsoft.AspNetCore.SignalR;
using SmartTask.Application.Interfaces;
using SmartTask.Domain.Entities;
using SmartTask.Infrastructure.Hubs;
using SmartTask.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Persistence.Services
{
    public class NotificationService: INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        public async Task SendNotificationAsync(Guid userId, string title, string message, string type)
        {
            var notification = new Notification
            {
                CompanyId = userId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _context.Notification.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new
            {
                id = notification.CompanyId,
                title = title,
                message = message,
                type = type,
                createdAt = notification.CreatedAt
            });
        }
    }
}
