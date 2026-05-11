using MediatR;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command
{
    public class ChangePasswordCommand :IRequest<Response<string>>
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; }   = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
