using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Application.Interfaces;

namespace SmartTask.Api.Filter
{
    public class HasPermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permission;

        public HasPermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new ForbidResult();
                return;
            }

            // ✅ Check token claims directly
            var hasPermissionClaim = user.Claims
                .Where(c => c.Type == "permission")
                .Any(c => c.Value == _permission);

            if (!hasPermissionClaim)
            {
                context.Result = new ForbidResult();
            }
        }
    }

}

//