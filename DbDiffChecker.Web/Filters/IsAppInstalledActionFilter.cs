using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;

namespace DbDiffChecker.Web.Filters
{
    public class IsAppInstalledActionFilter : IResourceFilter
    {
        private readonly IConfiguration _configuration;
        public IsAppInstalledActionFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var cnnStrUAT = _configuration.GetConnectionString("ConnectionString_UAT");
            var cnnStrProd = _configuration.GetConnectionString("ConnectionString_Prod");
            var isInstalled = !(string.IsNullOrEmpty(cnnStrUAT) || string.IsNullOrEmpty(cnnStrProd));

            var installPageUrl = "/install";
            var currentUrl = $"{context.HttpContext.Request.Path.ToUriComponent()}{context.HttpContext.Request.QueryString.ToUriComponent()}";

            if (!isInstalled && !currentUrl!.Contains(installPageUrl))
            {
                context.HttpContext.Response.Redirect(installPageUrl);
            }
            else if (isInstalled && currentUrl!.Contains(installPageUrl))
            {
                context.HttpContext.Response.Redirect("/");
            }
        }
    }
}
