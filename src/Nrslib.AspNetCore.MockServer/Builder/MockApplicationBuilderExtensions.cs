using System;
using Microsoft.AspNetCore.Builder;
using nrslib.AspNetCore.MockServer.Middlewares;

namespace nrslib.AspNetCore.MockServer.Builder
{
    public static class MockApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMock(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            return app.UseMiddleware<MockMiddleware>();
        }
    }
}
