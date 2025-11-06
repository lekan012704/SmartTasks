using MediatR;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command
{
    public class DectivateUserCommand : IRequest<Response<string>>
    {
        public string UserId { get; }

        public DectivateUserCommand(string userId)
        {
            UserId = userId;
        }
    }
}
