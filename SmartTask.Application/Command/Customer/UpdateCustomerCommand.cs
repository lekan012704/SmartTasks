using MediatR;
using SmartTask.Application.Dto.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command.Customer
{
    public class UpdateCustomerCommand :IRequest<Unit>
    {
        public UpdateCustomer Update { get; set; }
        public Guid Id { get; set; }

        public UpdateCustomerCommand(UpdateCustomer update, Guid id)
        {
            Update = update;
            Id = id;
        }
    }
}
