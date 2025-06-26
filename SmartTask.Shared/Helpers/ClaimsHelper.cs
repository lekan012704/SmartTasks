
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    namespace SmartTask.Shared.Helpers
    {
        public static class ClaimsHelper
        {
            public static List<string> GetPermissions(ClaimsPrincipal user)
            {
                return user?.Claims
                    .Where(c => c.Type == "permission")
                    .Select(c => c.Value)
                    .Distinct()
                    .ToList() ?? new List<string>();
            }

            public static string GetUserId(ClaimsPrincipal user)
            {
                return user?.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            }

            public static string GetUserRole(ClaimsPrincipal user)
            {
                return user?.Claims.FirstOrDefault(c => c.Type == "rolename")?.Value;
            }

            public static string GetEmail(ClaimsPrincipal user)
            {
                return user?.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            }

            public static string GetUsername(ClaimsPrincipal user)
            {
                return user?.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            }
        }
    }


