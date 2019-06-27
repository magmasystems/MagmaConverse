using System.Diagnostics;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MagmaConverse.Controllers
{
    public class ControllerLogger : ActionFilterAttribute
    {
        protected ILog Logger;
        protected Stopwatch Stopwatch;

        public ControllerLogger()
        {
            this.Logger = LogManager.GetLogger(this.GetType());
            this.Stopwatch = new Stopwatch();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!this.Logger.IsDebugEnabled)
                return;

            var actionDescriptor = context.ActionDescriptor;
            var controllerName = ((Controller)context.Controller).ControllerContext.ActionDescriptor.ControllerName;
            var actionName = actionDescriptor.DisplayName;
            var name = context.HttpContext.User.Identity.Name;
            var message = $"Executing Action: {actionName} on Controller: {controllerName} For User : {name}.";
            if (context.RouteData.Values["id"] != null)
            {
                var str = context.RouteData.Values["id"].ToString();
                message = $"Executing Action: {actionName} on Controller: {controllerName}  For User : {name} Given Route: {str}.";
            }
            this.Logger.Debug(message);
            this.Stopwatch.Restart();
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            if (!this.Logger.IsDebugEnabled)
                return;

            this.Stopwatch.Stop();
            var actionDescriptor = context.ActionDescriptor;
            var controllerName = ((Controller)context.Controller).ControllerContext.ActionDescriptor.ControllerName;
            var actionName = actionDescriptor.DisplayName;
            var name = context.HttpContext.User.Identity.Name;
            var message = $"Executed Action: {actionName} on Controller: {controllerName} For User : {name} took {this.Stopwatch.ElapsedMilliseconds} ms.";
            if (context.RouteData.Values["id"] != null)
            {
                var str = context.RouteData.Values["id"].ToString();
                message = $"Executed Action: {actionName} on Controller: {controllerName} For User : {name} Given Route: {str} took {this.Stopwatch.ElapsedMilliseconds} ms.";
            }
            this.Logger.Debug(message);
        }
    }
}

