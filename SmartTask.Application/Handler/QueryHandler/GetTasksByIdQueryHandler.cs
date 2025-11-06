using MediatR;
using Microsoft.Extensions.Logging;
using SmartTask.Application.Constants;
using SmartTask.Application.Dto.Task;
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
    public class GetTasksByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, Response<TaskDto>>
    {
        private readonly IEntityManagerAsync _entityManagerService; // Use your service interface
        private readonly ILogger<GetTaskByIdQueryHandler> _logger;

        public GetTasksByIdQueryHandler(IEntityManagerAsync entityManagerService, ILogger<GetTaskByIdQueryHandler> logger)
        {
            _entityManagerService = entityManagerService;
            _logger = logger;
        }

        public async Task<Response<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var taskDto = await _entityManagerService.GetTasksByIdAsync(request.TaskId);

                if (taskDto == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found.", request.TaskId);
                    return ApplicationConstants.FailureMessage<TaskDto>(null, "Task not found.");
                }

                return ApplicationConstants.SuccessMessage(taskDto, "Task retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task with ID {TaskId}", request.TaskId);
                return ApplicationConstants.FailureMessage<TaskDto>(null, "An error occurred while retrieving the task.");
            }
        }
    }
}
