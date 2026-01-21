using MediatR;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.QueryHandler
{
    public class GetProfileDetailsQueryHandler :IRequestHandler<GetProfileDetailsQuery, ProfileDetailsDto>
    {
        private readonly IEntityManagerAsync _entityManager;

        public GetProfileDetailsQueryHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<ProfileDetailsDto> Handle(GetProfileDetailsQuery request, CancellationToken cancellationToken)
        {
           return await _entityManager.GetProfileAsync();
        }
    }
}
