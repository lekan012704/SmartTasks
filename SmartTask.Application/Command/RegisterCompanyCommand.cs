using MediatR;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command
{
    public class RegisterCompanyCommand :IRequest<Response<CompanyResponse>>
    {
     public CompanyRequest CompanyRequest { get; set; }
        public RegisterCompanyCommand(CompanyRequest companyRequest)
        {
            CompanyRequest = companyRequest;    
        }

    }
}
