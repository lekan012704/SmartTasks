using MediatR;
using Microsoft.Extensions.Logging;
using SmartTask.Application.Constants;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using SmartTask.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTask.Application.Query.Task
{
    public class GetCompletedTasksQueryHandler : IRequestHandler<GetCompletedTasksQuery, Response<List<CompletedTaskDto>>>
    {
        private readonly IEntityManagerAsync _entityMnager;
        public GetCompletedTasksQueryHandler(
            IEntityManagerAsync entityMnager) 
        {
           
            _entityMnager = entityMnager;
        }

        public async Task<Response<List<CompletedTaskDto>>> Handle(GetCompletedTasksQuery request, CancellationToken cancellationToken)
        {

            return await _entityMnager.GetTasksCompletedAsync();
        }
    }
}
