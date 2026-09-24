using Feijuca.Auth.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Feijuca.Auth.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RequiredRoleAttribute(string role) : Attribute, IAuthorizationFilter
    {
        private readonly string _role = role;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            var realmRoles = user.FindFirst("realm_access")?.Value;
            if (realmRoles != null && realmRoles.Contains(_role))
            {
                return;
            }

            var clientRoles = user.FindFirst("resource_access")?.Value;
            if (clientRoles != null && clientRoles.Contains(_role))
            {
                return;
            }

            context.Result = new ObjectResult(FeijucaErrors.RequiredRoleError) { StatusCode = StatusCodes.Status403Forbidden };
        }
    }
}
