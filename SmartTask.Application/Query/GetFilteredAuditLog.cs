using MediatR;
using SmartTask.Application.Dto.Audit;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Query
{
    public class GetFilteredAuditLog :IRequest<Response<List<AuditLogDto>>>
    {
        public FilteredAuditLog FilteredAuditLog { get; set; }
        public GetFilteredAuditLog(FilteredAuditLog filteredAuditLog)
        {
            FilteredAuditLog = filteredAuditLog ?? throw new ArgumentNullException(nameof(filteredAuditLog));
        }
        
    }
}
