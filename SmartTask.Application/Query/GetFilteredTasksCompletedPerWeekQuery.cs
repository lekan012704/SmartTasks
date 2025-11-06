using MediatR;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Query
{
    public class GetFilteredTasksCompletedPerWeekQuery :IRequest<Response<List<TaskCompletionStatus>>>
    {
        public WeeklyStatsFilter Filter {  get; set; }
        public GetFilteredTasksCompletedPerWeekQuery(WeeklyStatsFilter weeklyStats) 
        {
          Filter = weeklyStats;
        }
    }
}
