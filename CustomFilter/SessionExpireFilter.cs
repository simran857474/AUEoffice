using System;
using System.Web;
using System.Web.Mvc;

public class SessionExpireFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        // Skip for login, logout, or any public page
        var controller = filterContext.RouteData.Values["controller"].ToString();
        var action = filterContext.RouteData.Values["action"].ToString();

        if (controller.Equals("Home", StringComparison.OrdinalIgnoreCase) &&
        (action.Equals("Login", StringComparison.OrdinalIgnoreCase) ||
         action.Equals("Logout", StringComparison.OrdinalIgnoreCase) ||
         action.Equals("ForgotPassword", StringComparison.OrdinalIgnoreCase) ||
         action.Equals("VerifyForgotPasswordOtp", StringComparison.OrdinalIgnoreCase)))
        { 
            return; // Skip filter for public actions
        }
        if (controller.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
          (action.Equals("UserDetails", StringComparison.OrdinalIgnoreCase) ||
          action.Equals("GetUserDetails", StringComparison.OrdinalIgnoreCase)))
        {
            return; // ✅ Skip authentication for this public action
        }
        if (HttpContext.Current.Session["UserId"] == null)
        {
            // Clear session before redirecting
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();

            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary {
                    { "controller", "Home" },
                    { "action", "Login" }
                });
        }

        base.OnActionExecuting(filterContext);
    }
}
