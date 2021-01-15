using System;
using FantasySoccer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace FantasySoccer.Authorization
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CustomAuthorize: Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            var tokenExpiration = filterContext.HttpContext.GetTokenAsync("TokenExpiration")?.Result;

            if (string.IsNullOrWhiteSpace(tokenExpiration) 
                    || DateTime.Compare(DateTime.Now, DateTime.Parse(tokenExpiration)) > 0 
                    || !filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.HttpContext.RequestServices.GetRequiredService<Services.IAuthenticationService>().LogoutAsync();

                filterContext.Result = new RedirectToRouteResult(new
                    RouteValueDictionary(new { controller = "Auth", action = "SignIn" }));
            }
        }
    }
}
