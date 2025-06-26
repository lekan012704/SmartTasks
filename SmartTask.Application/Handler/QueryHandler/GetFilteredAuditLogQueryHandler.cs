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
    public class GetFilteredAuditLogQueryHandler : IRequestHandler<GetFilteredAuditLog, Response<List<AuditLogDto>>>
    {
        private readonly IAuditLogRepository _auditLogRepository;
        public GetFilteredAuditLogQueryHandler(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }
        public async Task<Response<List<AuditLogDto>>> Handle(GetFilteredAuditLog request, CancellationToken cancellationToken)
        {
            return await _auditLogRepository.GetFilteredAsync(request.FilteredAuditLog);
        }
    }
}
