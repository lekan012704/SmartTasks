using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command
{

    public class UpdateProfileCommand : IRequest<Unit>
    {
        public string ContactEmail { get; set; }
        public string PhoneNumber { get; set; }
        public string PrimaryAddress { get; set; }
    }
}
