using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command.Customer
{
    public class DeleteCustomerCommand : IRequest<Unit>
    {
        public Guid CustomerId { get; set; }
    }
}
