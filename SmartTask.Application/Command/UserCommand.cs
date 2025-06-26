using MediatR;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command
{
    public class UserCommand : IRequest<Response<UserResponseDto>>
    {
        public UserRequestDto UserRequest { get; set; }
        public UserCommand(UserRequestDto userRequest)
        {
            UserRequest = userRequest;
        }
    }
}
