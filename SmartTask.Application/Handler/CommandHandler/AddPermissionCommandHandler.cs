using MediatR;
using SmartTask.Application.Command;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.CommandHandler
{
    public class AddPermissionCommandHandler :IRequestHandler<AddPermissionCommand,Response<List<string>>>
    {
       private readonly IAccountService _account;
        public AddPermissionCommandHandler(IAccountService account)
        {
            _account = account;
        }
        public async Task<Response<List<string>>> Handle(AddPermissionCommand request, CancellationToken cancellationToken)
        {
            return await _account.AddPermissionAsync(request.Permission);
        }
    }
}
