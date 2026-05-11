using MediatR;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command
{
    public class ResetPasswordCommand : IRequest<Response<string>>
    {
        public string Email { get; set; }
    }
}
