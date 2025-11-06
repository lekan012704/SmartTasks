using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
// Added for JsonResult and Response
using SmartTask.Application.Wrappers;

namespace SmartTask.Api.Filter
{
    public class HasPermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permission;

        public HasPermissionAttribute(string permission)
        {
            _permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var logger = context.HttpContext.RequestServices
                                .GetService<ILogger<HasPermissionAttribute>>();
            try
            {
                var user = context.HttpContext.User;

                if (user.Identity == null || !user.Identity.IsAuthenticated)
                {
                    // This is a 401 Unauthorized, which is fine
                    context.Result = new UnauthorizedResult();
                    return Task.CompletedTask;
                }

                // Use HasClaim for a direct and fast check
                if (!user.HasClaim("permission", _permission))
                {
               
                    var errorResponse = new Response<string>(
                        $"You are not authorized to access this resource. Missing permission: '{_permission}'"
                    );
                    errorResponse.Succeeded = false; // Set Succeeded to false for failures

                    context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Result = new JsonResult(errorResponse);

                    return Task.CompletedTask;
                }

                // If we reach here, the user is authorized.
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Authorization error in HasPermissionAttribute: {Message}", ex.Message);
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return Task.CompletedTask;
        }
    }
}

