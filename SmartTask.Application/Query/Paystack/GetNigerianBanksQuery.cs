using MediatR;
using SmartTask.Application.Dto.Paystack;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Query.Paystack
{
    public record GetNigerianBanksQuery : IRequest<Response<List<BankDto>>>;
}
