using MediatR;
using SmartTask.Application.Command.Customer;
using SmartTask.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.CommandHandler.Customer
{
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Unit>
    {
        private readonly IEntityManagerAsync _entityManager;
        public UpdateCustomerCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<Unit> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            return await _entityManager.UpdateCustomerAsync(request);
        }
    }
}
