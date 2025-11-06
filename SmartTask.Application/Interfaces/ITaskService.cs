using SmartTask.Application.Command.Task;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Interfaces
{
    public interface ITaskService
    {
        Task<Response<CreateTaskResponse>> CreateTaskAsync(CreateTaskDto request);
        Task<Response<TaskDto>> UpdateTaskAsync(TaskUpdate request);
        Task<Response<bool>> DeleteTaskAsync(DeleteTaskCommand request);
    }
}
