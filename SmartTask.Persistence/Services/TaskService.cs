using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartTask.Application.Command.Task;
using SmartTask.Application.Constants;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Enums;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Query;
using SmartTask.Application.Wrappers;
using SmartTask.Domain.Entities;
using SmartTask.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Persistence.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TaskService> _logger;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IAuditLogRepository _auditLogRepository;

        public TaskService(IUnitOfWork unitOfWork, ILogger<TaskService> logger, IAuthenticatedUserService authenticatedUserService, IAuditLogRepository auditLogRepository)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _authenticatedUserService = authenticatedUserService;
            _auditLogRepository = auditLogRepository;
        }
        public async Task<Response<CreateTaskResponse>> CreateTaskAsync(CreateTaskDto request)
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    return ApplicationConstants.FailureMessage<CreateTaskResponse>(null,
                        "Unable to determine your company context.");
                }

                if (await _unitOfWork.Tasks.TaskExistsAsync(request.Title, companyId))
                {
                    _logger.LogWarning(
                        "TaskExitsAsync: Task '{Task}' already exists in company '{CompanyId}'",
                        request.Title, companyId);

                    return ApplicationConstants.FailureMessage<CreateTaskResponse>(null,
                        $"A task with title '{request.Title}' already exists in your company.");
                }

                var task = new TaskItem
                {
                    Title = request.Title,
                    Description = request.Description,
                    CreatedBy = _authenticatedUserService.UserId,
                    CompanyId = companyId,
                    CreatedAt = DateTime.UtcNow,
                    AssignedUserId = request.AssignedUserId,
                    DueDate = request.DueDate,
                    Priority = Enum.TryParse(request.Priority, true, out TaskPriority priority) ? priority : TaskPriority.Low,
                    Status = Enum.TryParse(request.Status, true, out TaskStatuses status) ? status : TaskStatuses.New,
                    IsCompleted = false,
                    isActive = true
                };

                await _unitOfWork.Tasks.AddAsync(task);
                var log = new TaskActivity
                {
                    TaskId = task.Id,
                    Action = "Task Created",
                    PerformedBy = _authenticatedUserService.UserId,
                    NewValue = $"Created task: {request.Title}"
                };

                await _auditLogRepository.InsertAsync(log);
                await _unitOfWork.SaveChangesAsync();

                var response = new CreateTaskResponse
                {
                    TaskId = task.Id,
                    Title = task.Title,
                    AssignedUserId = task.AssignedUserId,
                    Status = task.Status.ToString(),
                    DateCreated = task.CreatedAt
                };

                return ApplicationConstants.SuccessMessage(response, "Task created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding task: {Message}", ex.Message);
                return ApplicationConstants.FailureMessage<CreateTaskResponse>(null, "An error occurred while adding the task.");
            }
        }
        public async Task<Response<TaskDto>> UpdateTaskAsync(TaskUpdate request)
        {
            try
            {
                var companyId = Guid.Parse(_authenticatedUserService.CompanyId!);

                var task = await _unitOfWork.Tasks
                    .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.CompanyId == companyId);

                if (task == null)
                    return ApplicationConstants.FailureMessage<TaskDto>(null, "Task not found for this company.");

                var oldValues = new Dictionary<string, string>();
                var newValues = new Dictionary<string, string>();

                if (!string.IsNullOrWhiteSpace(request.Title) && task.Title != request.Title)
                {
                    oldValues["Title"] = task.Title;
                    newValues["Title"] = request.Title;
                    task.Title = request.Title;
                }

                if (!string.IsNullOrWhiteSpace(request.Description) && task.Description != request.Description)
                {
                    oldValues["Description"] = task.Description;
                    newValues["Description"] = request.Description;
                    task.Description = request.Description;
                }

                if (!string.IsNullOrWhiteSpace(request.Status) && task.Status.ToString() != request.Status)
                {
                    oldValues["Status"] = task.Status.ToString();
                    task.Status = (TaskStatuses)Enum.Parse<TaskStatus>(request.Status, true);
                    newValues["Status"] = task.Status.ToString();
                }

                if (!string.IsNullOrWhiteSpace(request.Priority) && task.Priority.ToString() != request.Priority)
                {
                    oldValues["Priority"] = task.Priority.ToString();
                    task.Priority = Enum.Parse<TaskPriority>(request.Priority, true);
                    newValues["Priority"] = task.Priority.ToString();
                }

                if (!string.IsNullOrWhiteSpace(request.AssignedUserId.ToString()) && task.AssignedUserId != request.AssignedUserId)
                {
                    oldValues["AssignedUserId"] = task.AssignedUserId.ToString();
                    newValues["AssignedUserId"] = request.AssignedUserId.ToString();
                    task.AssignedUserId = request.AssignedUserId;
                }

                if (request.DueDate.HasValue && task.DueDate != request.DueDate)
                {
                    oldValues["DueDate"] = task.DueDate.ToString("yyyy-MM-dd") ?? "null";
                    newValues["DueDate"] = request.DueDate?.ToString("yyyy-MM-dd") ?? "null";
                    task.DueDate = request.DueDate.Value;
                }

                await _unitOfWork.SaveChangesAsync();

                await _auditLogRepository.InsertAsync(new TaskActivity
                {
                    TaskId = task.Id,
                    Action = "TaskUpdated",
                    PerformedBy = request.UpdatedBy,
                    OldValue = JsonConvert.SerializeObject(oldValues),
                    NewValue = JsonConvert.SerializeObject(newValues)
                });

                var taskDto = new TaskDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    AssignedUserId = task.AssignedUserId,
                    DueDate = task.DueDate,
                    Priority = task.Priority.ToString(),
                    Status = task.Status.ToString(),
                    CreatedBy = task.CreatedBy,
                    IsActive = true,
                    IsCompleted = task.IsCompleted,
                    CreatedAt = task.CreatedAt
                };
                return ApplicationConstants.SuccessMessage(taskDto, "Task updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task (ID: {TaskId})", request.TaskId);
                return ApplicationConstants.FailureMessage<TaskDto>(null, "An error occurred while updating the task.");
            }
        }
        public async Task<Response<bool>> DeleteTaskAsync(DeleteTaskCommand request)
        {
            try
            {
                var companyId = Guid.Parse(_authenticatedUserService.CompanyId!);

                var task = await _unitOfWork.Tasks
                    .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.CompanyId == companyId && !t.IsDeleted);

                if (task == null)
                {
                    return ApplicationConstants.FailureMessage<bool>(false, "Task not found or already deleted.");
                }

                var oldValue = JsonConvert.SerializeObject(task);

                task.IsDeleted = true;
                task.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                await _auditLogRepository.InsertAsync(new TaskActivity
                {
                    TaskId = task.Id,
                    Action = "TaskDeleted",
                    PerformedBy = _authenticatedUserService.UserId,
                    OldValue = oldValue,
                    NewValue = null
                });

                return ApplicationConstants.SuccessMessage(true, "Task deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task (ID: {TaskId})", request.TaskId);
                return ApplicationConstants.FailureMessage<bool>(false, "An error occurred while deleting the task.");
            }
        }

    }
}
