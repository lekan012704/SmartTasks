using MediatR;
using SmartTask.Application.Dto;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Query
{
    public class GetUserNotificationsQuery : IRequest<Response<List<NotificationDto>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool OnlyUnread { get; set; } = false;
    }
}
