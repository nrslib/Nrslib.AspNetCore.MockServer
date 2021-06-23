using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using File = System.IO.File;


namespace nrslib.AspNetCore.MockServer.Middlewares
{
    public class MockMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration configuration;

        public MockMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.next = next;
            this.configuration = configuration;
        }

        public async Task Invoke(HttpContext context, IHostingEnvironment env)
        {
            var controllerName = (string) context.GetRouteValue("controller");
            var actionName = (string) context.GetRouteValue("action");

            if (controllerName is null || actionName is null)
            {
                await next(context);
                return;
            }

            var (targetFileExists, filePath) = GetTargetFilePath(env, controllerName, actionName);

            if (!targetFileExists)
            {
                await next(context);
                return;
            }

            var content = await File.ReadAllTextAsync(filePath);
            if (ShouldIgnore(content))
            {
                await next(context);
                return;
            }

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(content);
        }

        private (bool, string) GetTargetFilePath(IHostingEnvironment env,string controllerName, string actionName)
        {
            var relationalPath = configuration["MockServerConfig:MockRootPath"] ?? "Mock";
            var filePath = Path.Combine(env.ContentRootPath, relationalPath, controllerName, actionName + ".json");

            return File.Exists(filePath) 
                ? (true, filePath) 
                : (false, null);
        }

        private bool ShouldIgnore(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                if (!bool.TryParse(configuration["MockServerConfig:IgnoreBlank"], out var ignoreBlank))
                {
                    return true;
                }

                if (ignoreBlank)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
