using MediatR;
using SmartTask.Application.Dto.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Query
{
    public record GetProfileDetailsQuery :IRequest<ProfileDetailsDto>;
}
