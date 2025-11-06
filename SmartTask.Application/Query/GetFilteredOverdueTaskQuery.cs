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
    public class GetFilteredOverdueTaskQuery : IRequest<Response<List<OverdueTaskStatsDto>>>
    {
        public FilteredOverdueTask filtered { get; set; }
        public GetFilteredOverdueTaskQuery(FilteredOverdueTask filteredOverdue)
        {
            filtered = filteredOverdue;
        }
    }
}
