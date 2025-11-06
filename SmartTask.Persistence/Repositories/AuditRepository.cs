using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTask.Application.Constants;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using SmartTask.Domain.Entities;
using SmartTask.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SmartTask.Domain.Constants.Permissions;

namespace SmartTask.Persistence.Repositories
{
    public class AuditRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditRepository> _logger;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IUnitOfWork _unitOfWork;
        public AuditRepository(ApplicationDbContext context, ILogger<AuditRepository> logger,IAuthenticatedUserService authenticatedUserService,IUnitOfWork unitOfWork)
        {
            _context = context;
            _logger = logger;
            _authenticatedUserService = authenticatedUserService;
            _unitOfWork = unitOfWork;
        }
        //public async Task<Response<string>> InsertAsync(TaskActivity log)
        //{
        //    try
        //    {
        //        if (log == null)
        //            return ApplicationConstants.FailureMessage<string>(null, "Audit log cannot be null.");

        //        var auditLog = new AuditLog
        //        {
        //            Action = log.Action,
        //            TaskId = log.TaskId,
        //            PerformedBy = log.PerformedBy,
        //            OldValue = log.OldValue,
        //            NewValue = log.NewValue,
        //            PerformedAt = DateTime.UtcNow   
        //        };

        //        await _unitOfWork.Audit.AddAsync(auditLog);

        //        return ApplicationConstants.SuccessMessage("Audit log inserted successfully.");
        //    }
        //    catch (Exception ex)
        //    {

        //        return ApplicationConstants.FailureMessage<string>(null, "An error occurred while saving audit log.");
        //    }
        //}
        //public async Task<Response<List<AuditLogDto>>> GetAllAsync()
        //{
        //    try
        //    {
        //        var auditEntities = await _unitOfWork.Audit.GetAllAsync();

        //        var auditDtos = auditEntities.Select(a => new AuditLogDto
        //        {
        //            Id = a.Id,
        //            Action = a.Action,
        //            TaskId = a.TaskId,
        //            PerformedBy = a.PerformedBy,
        //            OldValue = a.OldValue,
        //            NewValue = a.NewValue,
        //            PerformedAt = a.PerformedAt
        //        }).ToList();

        //        return ApplicationConstants.SuccessMessage(auditDtos, "Audit logs retrieved successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApplicationConstants.FailureMessage<List<AuditLogDto>>(null, "An error occurred while retrieving audit logs.");
        //    }
        //}

        //public async Task<Response<List<AuditLogDto>>> GetFilteredAsync(FilteredAuditLog request)
        //{
        //    try
        //    {
        //        var companyId = Guid.Parse(_authenticatedUserService.CompanyId);
        //        var query = _context.AuditLog
        //            .Where(a => _context.TaskItem
        //                .Any(t => t.Id == a.TaskId && t.CompanyId == companyId));

        //        if (request.TaskId.HasValue)
        //            query = query.Where(a => a.TaskId == request.TaskId.Value);

        //        if (!string.IsNullOrEmpty(request.PerformedBy))
        //            query = query.Where(a => a.PerformedBy == request.PerformedBy);

        //        if (request.startDate.HasValue)
        //            query = query.Where(a => a.PerformedAt >= request.startDate.Value);

        //        if (request.endDate.HasValue)
        //            query = query.Where(a => a.PerformedAt <= request.endDate.Value);

        //        var auditLogs = await query
        //            .OrderByDescending(a => a.PerformedAt)
        //            .Select(a => new AuditLogDto
        //            {
        //                Id = a.Id,
        //                Action = a.Action,
        //                TaskId = a.TaskId,
        //                PerformedBy = a.PerformedBy,
        //                OldValue = a.OldValue,
        //                NewValue = a.NewValue,
        //                PerformedAt = a.PerformedAt
        //            })
        //            .ToListAsync();

        //        return ApplicationConstants.SuccessMessage(auditLogs, "Audit logs retrieved successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApplicationConstants.FailureMessage<List<AuditLogDto>>(null, "An error occurred while retrieving audit logs.");
        //    }
        //}
    }
}
