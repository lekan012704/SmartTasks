using MediatR;
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
    public class GetAllPermissionsQueryHandler  : IRequestHandler<GetAllPermissionsQuery, Response<List<string>>>
    {
        private readonly IPermissionService _permissionService;

        public GetAllPermissionsQueryHandler(IPermissionService permissionService)
        {
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }

        public async Task<Response<List<string>>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
        {
            var permissions = await _permissionService.GetPermissionsAsync(request.SearchTerm);

            if (!permissions.Any() && !string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                return Response<List<string>>.Success(new List<string>(), $"No permissions found matching '{request.SearchTerm}'.");
            }

            return Response<List<string>>.Success(permissions, "Successfully retrieved permissions.");
        }
    }
}
