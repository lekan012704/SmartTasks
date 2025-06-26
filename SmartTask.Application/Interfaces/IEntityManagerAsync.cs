using SmartTask.Application.Command;
using SmartTask.Application.Command.Task;
using SmartTask.Application.Dto;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Query;
using SmartTask.Application.Wrappers;
using SmartTask.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Interfaces
{
    public interface IEntityManagerAsync
    {
        Task<Response<CompanyResponse>> RegisterCompanyAsync(CompanyRequest request);
        Task<Response<UserResponseDto>> RegisterUserAsync(UserRequestDto request);
        Task<Response<List<TaskDto>>> GetTaskByCompanyIdAsync(GetTaskByIdQuery request);
        Task<Response<List<TaskDto>>> GetTasksByAssignedUserAsync(GetTasksByAssignedUserQuery request);
        Task<Response<List<TaskCompletionStatus>>> GetTasksCompletedPerWeekAsync();
        Task<Response<List<TaskCompletionStatus>>> GetFilteredTasksAsync(WeeklyStatsFilter request);
        Task<Response<List<OverdueTaskStatsDto>>> GetOverdueTasksAsync();
        Task<Response<List<OverdueTaskStatsDto>>> GetFilteredOverdueTasksAsync(FilteredOverdueTask request);



    }
}
 