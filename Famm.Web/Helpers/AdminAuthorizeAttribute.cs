using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Famm.Web.Helpers
{
    /// <summary>
    /// Атрибут авторизации для ограничения доступа только администраторам
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                return false;
            
            // Проверяем роль пользователя
            var userRole = httpContext.Session["UserRole"] as string;
            return !string.IsNullOrEmpty(userRole) && userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }
        
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // Пользователь не авторизован, перенаправляем на страницу входа
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new
                        {
                            controller = "AuthFlow",
                            action = "Login",
                            returnUrl = filterContext.HttpContext.Request.Url.PathAndQuery
                        }));
            }
            else
            {
                // Пользователь авторизован, но не имеет прав доступа
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new
                        {
                            controller = "Home",
                            action = "AccessDenied"
                        }));
            }
        }
    }
} 