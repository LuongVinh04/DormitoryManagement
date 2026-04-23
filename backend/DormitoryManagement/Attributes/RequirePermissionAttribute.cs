using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace DormitoryManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string permission) : base(typeof(RequirePermissionFilter))
        {
            Arguments = new object[] { permission };
        }
    }

    public class RequirePermissionFilter : IAuthorizationFilter
    {
        private readonly string _permission;

        public RequirePermissionFilter(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var permissionsClaim = user.Claims.FirstOrDefault(c => c.Type == "permissions");
            if (permissionsClaim == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var permissions = permissionsClaim.Value.Split(',');
            if (!permissions.Contains(_permission))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
