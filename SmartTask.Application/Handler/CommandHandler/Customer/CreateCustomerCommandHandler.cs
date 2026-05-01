using MediatR;
using SmartTask.Application.Command.Customer;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.CommandHandler.Customer
{
    public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand,Response<string>>
    {
        private readonly IEntityManagerAsync _entityManager;
        public CreateCustomerCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<Response<string>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            return await _entityManager.AddCustomerAsync(request.Request);
        }
    }
}
