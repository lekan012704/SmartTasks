using MediatR;
using SmartTask.Application.Command;
using SmartTask.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.CommandHandler
{
    public class UpdateProfileCommandHandler :IRequestHandler<UpdateProfileCommand, Unit>
    {
        private readonly IEntityManagerAsync _entityManager;

        public UpdateProfileCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
       public async Task<Unit> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
          return await _entityManager.UpdateProfileAsync(request);
          
        }
    }
}
