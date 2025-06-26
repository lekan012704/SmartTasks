using MediatR;
using SmartTask.Application.Dto.Audit;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Query;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.QueryHandler
{
    public class GetAuditQueryHandler :IRequestHandler<GetAuditQuery, Response<List<AuditLogDto>>>
    {
        private readonly IAuditLogRepository auditLogRepository;
        public GetAuditQueryHandler(IAuditLogRepository auditLogRepository)
        {
            auditLogRepository = auditLogRepository;
        }
        public async Task<Response<List<AuditLogDto>>> Handle(GetAuditQuery request, CancellationToken cancellationToken)
        {
            return await auditLogRepository.GetAllAsync();
        }
    }
}
