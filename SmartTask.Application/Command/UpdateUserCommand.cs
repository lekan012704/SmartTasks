using MediatR;
using SmartTask.Application.Dto;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command
{
    public class UpdateUserCommand : IRequest<Response<string>>
    {
        public UpdateUserRequestDto UpdateUser { get; set; }
        public string UserId { get; set; }
        public UpdateUserCommand(UpdateUserRequestDto updateUserRequestDto,string userId) 
        {
          UpdateUser = updateUserRequestDto;
            UserId = userId;
        }
    }
}
