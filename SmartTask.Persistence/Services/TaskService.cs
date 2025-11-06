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
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return ApplicationConstants.FailureMessage<CreateTaskResponse>(null, "Title is required.");
                }

                if (string.IsNullOrWhiteSpace(request.Description))
                {
                    return ApplicationConstants.FailureMessage<CreateTaskResponse>(null, "Description is required.");
                }

                if (string.IsNullOrWhiteSpace(request.Priority))
                {
                    return ApplicationConstants.FailureMessage<CreateTaskResponse>(null, "Priority is required.");
                }

                if (string.IsNullOrWhiteSpace(request.Status))
                {
                    return ApplicationConstants.FailureMessage<CreateTaskResponse>(null, "Status is required.");
                }

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
                    AssignedUserId = _authenticatedUserService.UserId,
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
                // --- Validate CompanyId ---
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    _logger.LogWarning("UpdateTaskAsync: Could not parse CompanyId from authenticated user.");
                    return ApplicationConstants.FailureMessage<TaskDto>(null, "Invalid company context.");
                }

                // --- Fetch the Task ---
                // Assuming TaskItem has a string AssignedUserId
                var task = await _unitOfWork.Tasks
                    .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.CompanyId == companyId && !t.IsDeleted); // Ensure not soft-deleted

                if (task == null)
                    return ApplicationConstants.FailureMessage<TaskDto>(null, "Task not found for this company or has been deleted.");

                // --- Prepare for Audit Logging ---
                var oldValues = new Dictionary<string, string?>(); // Allow null for comparison
                var newValues = new Dictionary<string, string?>();
                bool changesMade = false; // Flag to track if any property was changed

                // --- Update Properties ---

                // Title
                if (!string.IsNullOrWhiteSpace(request.Title) && task.Title != request.Title)
                {
                    oldValues["Title"] = task.Title;
                    task.Title = request.Title;
                    newValues["Title"] = task.Title;
                    changesMade = true;
                }

                // Description
                var currentDescription = task.Description ?? "";
                var requestedDescription = request.Description ?? "";
                if (currentDescription != requestedDescription)
                {
                    if (!string.IsNullOrWhiteSpace(requestedDescription))
                    {
                        oldValues["Description"] = task.Description;
                        task.Description = requestedDescription;
                        newValues["Description"] = task.Description;
                        changesMade = true;
                    }
                    else if (task.Description != null)
                    {
                        oldValues["Description"] = task.Description;
                        task.Description = null;
                        newValues["Description"] = null;
                        changesMade = true;
                    }
                }


                // Status & IsCompleted
                bool statusChangedToCompleted = false;
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    if (Enum.TryParse<TaskStatuses>(request.Status, true, out var newStatus))
                    {
                        if (task.Status != newStatus)
                        {
                            oldValues["Status"] = task.Status.ToString();
                            task.Status = newStatus;
                            newValues["Status"] = task.Status.ToString();
                            changesMade = true;
                            if (newStatus == TaskStatuses.Completed)
                            {
                                statusChangedToCompleted = true;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("UpdateTaskAsync: Invalid status value provided: {Status}", request.Status);
                    }
                }

                // Priority
                if (!string.IsNullOrWhiteSpace(request.Priority))
                {
                    if (Enum.TryParse<TaskPriority>(request.Priority, true, out var newPriority))
                    {
                        if (task.Priority != newPriority)
                        {
                            oldValues["Priority"] = task.Priority.ToString();
                            task.Priority = newPriority;
                            newValues["Priority"] = task.Priority.ToString();
                            changesMade = true;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("UpdateTaskAsync: Invalid priority value provided: {Priority}", request.Priority);
                    }
                }


                // --- UPDATED AssignedUserId LOGIC (assuming both request.AssignedUserId and task.AssignedUserId are string) ---
                // Check if the request provided a non-empty AssignedUserId string
                if (!string.IsNullOrWhiteSpace(request.AssignedUserId))
                {
                    // Attempt to parse the string from the request into a Guid for validation
                    if (Guid.TryParse(request.AssignedUserId, out var parsedRequestedGuid))
                    {
                        // Compare the *string* from the request with the *string* currently on the task
                        if (task.AssignedUserId != request.AssignedUserId)
                        {
                            // Optional: Validate if the new user ID exists
                            // var userExists = await _context.Users.AnyAsync(u => u.Id == request.AssignedUserId); // Compare string IDs
                            // if (!userExists) return ApplicationConstants.FailureMessage<TaskDto>(null, "Assigned user not found.");

                            oldValues["AssignedUserId"] = task.AssignedUserId; // Record old string value
                            task.AssignedUserId = request.AssignedUserId; // Assign new string value
                            newValues["AssignedUserId"] = task.AssignedUserId; // Record new string value
                            changesMade = true;
                        }
                    }
                    else
                    {
                        // Log a warning if the provided string wasn't a valid Guid format
                        _logger.LogWarning("UpdateTaskAsync: Invalid Guid format for AssignedUserId: {AssignedUserId}", request.AssignedUserId);
                        // Optionally return ApplicationConstants.FailureMessage<TaskDto>(null, "Invalid Assigned User ID format.");
                    }
                }
                // Handle case where request explicitly sets AssignedUserId to null/empty
                else if (!string.IsNullOrEmpty(task.AssignedUserId)) // Check if current value wasn't already null/empty
                {
                    oldValues["AssignedUserId"] = task.AssignedUserId;
                    task.AssignedUserId = null; // Set to null
                    newValues["AssignedUserId"] = null;
                    changesMade = true;
                }
                // --- END UPDATED AssignedUserId LOGIC ---


                // DueDate
                if (request.DueDate.HasValue && task.DueDate != request.DueDate.Value)
                {
                    oldValues["DueDate"] = task.DueDate?.ToString("o");
                    task.DueDate = request.DueDate.Value;
                    newValues["DueDate"] = task.DueDate?.ToString("o");
                    changesMade = true;
                }
                else if (!request.DueDate.HasValue && task.DueDate.HasValue)
                {
                    oldValues["DueDate"] = task.DueDate?.ToString("o");
                    task.DueDate = null;
                    newValues["DueDate"] = null;
                    changesMade = true;
                }


                // Update IsCompleted flag if Status changed to Completed
                if (statusChangedToCompleted && !task.IsCompleted)
                {
                    oldValues["IsCompleted"] = task.IsCompleted.ToString();
                    task.IsCompleted = true;
                    newValues["IsCompleted"] = task.IsCompleted.ToString();
                    changesMade = true;
                }
                else if (!statusChangedToCompleted && task.IsCompleted && oldValues.ContainsKey("Status") && oldValues["Status"] == TaskStatuses.Completed.ToString())
                {
                    oldValues["IsCompleted"] = task.IsCompleted.ToString();
                    task.IsCompleted = false;
                    newValues["IsCompleted"] = task.IsCompleted.ToString();
                    changesMade = true;
                }

                // Update timestamp only if changes were made
                if (changesMade)
                {
                    task.UpdatedAt = DateTime.UtcNow;
                }

                // --- Save Changes (only if needed) ---
                if (changesMade)
                {
                    await _unitOfWork.SaveChangesAsync();

                    // --- Audit Log (only if changes were made) ---
                    await _auditLogRepository.InsertAsync(new TaskActivity
                    {
                        TaskId = task.Id,
                        Action = "TaskUpdated",
                        PerformedBy = _authenticatedUserService.UserId,
                        OldValue = JsonConvert.SerializeObject(oldValues),
                        NewValue = JsonConvert.SerializeObject(newValues)
                    });
                }
                else
                {
                    _logger.LogInformation("UpdateTaskAsync: No changes detected for task (ID: {TaskId})", request.TaskId);
                }


             
                var taskDto = new TaskDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    AssignedUserId = task.AssignedUserId, // Send back the string ID
                    AssignedUserName = _authenticatedUserService.UserName ?? "Unassigned",
                    DueDate = task.DueDate,
                    Priority = task.Priority.ToString(),
                    Status = task.Status.ToString(),
                    CreatedBy = task.CreatedBy,
                    IsActive = task.isActive,
                    IsCompleted = task.IsCompleted,
                    CreatedAt = task.CreatedAt
                };
                return ApplicationConstants.SuccessMessage(taskDto, changesMade ? "Task updated successfully." : "No changes detected.");
            }
            catch (FormatException ex) // Catch specific parsing errors
            {
                _logger.LogError(ex, "Error parsing input during task update (ID: {TaskId})", request.TaskId);
                return ApplicationConstants.FailureMessage<TaskDto>(null, "Invalid data format provided for update.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task (ID: {TaskId})", request.TaskId);
                return ApplicationConstants.FailureMessage<TaskDto>(null, "An error occurred while updating the task.");
            }
        }

        // Assume other methods like CompleteTaskAsync are also in this service class
        public async Task<Response<bool>> CompleteTaskAsync(Guid taskId, string userId, Guid companyId)
        {
            try
            {
                var task = await _unitOfWork.Tasks
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.CompanyId == companyId && !t.IsDeleted);

                if (task == null)
                {
                    return ApplicationConstants.FailureMessage<bool>(false, "Task not found or already deleted.");
                }

                if (task.IsCompleted)
                {
                    // Task is already completed, maybe return success or a specific message
                    _logger.LogInformation("CompleteTaskAsync: Task (ID: {TaskId}) is already completed.", taskId);
                    return ApplicationConstants.SuccessMessage(true, "Task is already completed.");
                }

                var oldStatus = task.Status.ToString();
                var oldIsCompleted = task.IsCompleted.ToString();

                task.Status = TaskStatuses.Completed;
                task.IsCompleted = true;
                task.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                // Audit Log
                await _auditLogRepository.InsertAsync(new TaskActivity
                {
                    TaskId = task.Id,
                    Action = "TaskCompleted",
                    PerformedBy = userId,
                    OldValue = JsonConvert.SerializeObject(new { Status = oldStatus, IsCompleted = oldIsCompleted }),
                    NewValue = JsonConvert.SerializeObject(new { Status = task.Status.ToString(), IsCompleted = task.IsCompleted.ToString() })
                });

                return ApplicationConstants.SuccessMessage(true, "Task marked as completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing task (ID: {TaskId})", taskId);
                return ApplicationConstants.FailureMessage<bool>(false, "An error occurred while completing the task.");
            }
        }

        // Assume other methods like CompleteTaskAsync are also in this service class
        // ...

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
