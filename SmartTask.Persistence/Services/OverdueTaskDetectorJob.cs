using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Enums;
using SmartTask.Application.Interfaces;
using SmartTask.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Persistence.Services
{
    public class OverdueTaskDetectorJob
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IAuditLogRepository _auditLogRepo;
        private readonly ILogger<OverdueTaskDetectorJob> _logger;

        public OverdueTaskDetectorJob(
            ApplicationDbContext context,
            IEmailService emailService, 
            IAuditLogRepository auditLogRepo,
            ILogger<OverdueTaskDetectorJob> logger)
        {
            _context = context;
            _emailService = emailService;
            _auditLogRepo = auditLogRepo;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var now = DateTime.UtcNow;

                var tasksToNotify = await _context.TaskItem
                    .Where(t => !t.IsDeleted &&
                                !t.OverdueReminderSent &&
                                t.DueDate < now &&
                                t.Status != TaskStatuses.Completed)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} tasks that are overdue and need reminders.", tasksToNotify.Count);

                foreach (var task in tasksToNotify)
                {
                    try
                    {
                        await _emailService.SendEmailAsync(task.AssignedUserId.ToString(),
                            "Task Overdue Reminder",
                            $"Your task \"{task.Title}\" is now overdue. Please review and update it.");

                        _logger.LogInformation("Reminder email sent to {Email} for task {TaskId}", task.AssignedUserId, task.Id);

                        await _auditLogRepo.InsertAsync(new TaskActivity
                        {
                            TaskId = task.Id,
                            Action = "OverdueReminderSent",
                            PerformedBy = "System",
                            Description = $"Reminder sent for overdue task '{task.Title}'"
                        });

                        _logger.LogInformation("Audit log created for overdue reminder for task {TaskId}", task.Id);

                        task.OverdueReminderSent = true;
                    }
                    catch (Exception innerEx)
                    {
                        _logger.LogError(innerEx, "Failed to send reminder or log audit for task {TaskId} ({Email})", task.Id, task.AssignedUserId);
                        // Optionally: add a retry flag, or move task to a failed queue for future attempts
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing OverdueTaskDetectorJob.");
            }
        }
    }

}
