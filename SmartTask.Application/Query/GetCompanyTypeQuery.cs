using MediatR;
using SmartTask.Application.Dto;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Query
{
    public class GetCompanyTypesQuery : IRequest<Response<List<CompanyTypeDto>>> { }
}
