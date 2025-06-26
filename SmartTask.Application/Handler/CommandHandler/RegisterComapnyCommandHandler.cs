using MediatR;
using SmartTask.Application.Command;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.CommandHandler
{
    public class RegisterComapnyCommandHandler : IRequestHandler<RegisterCompanyCommand, Response<CompanyResponse>>
    {
        private readonly IEntityManagerAsync _entityManager;
        public RegisterComapnyCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<Response<CompanyResponse>> Handle(RegisterCompanyCommand request, CancellationToken cancellationToken)
        {
            return await _entityManager.RegisterCompanyAsync(request.CompanyRequest);
        }
    }
}
