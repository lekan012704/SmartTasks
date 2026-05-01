using MediatR;
using SmartTask.Application.Dto.Customer;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command.Customer
{
    public class CreateCustomerCommand : IRequest<Response<string>>
    {
        public Customerrequest Request { get; set; }

        public CreateCustomerCommand(Customerrequest request)
        {
            Request = request;
        }
    }
}
