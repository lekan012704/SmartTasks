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
    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Unit>
    {
        private readonly IEntityManagerAsync _entityManager;
        public DeleteCustomerCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<Unit> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            await _entityManager.DeleteCustomerAsync(request);
            return Unit.Value;
        }
    }
}
