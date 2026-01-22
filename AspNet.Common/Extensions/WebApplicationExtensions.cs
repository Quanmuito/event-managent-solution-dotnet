namespace AspNet.Common.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

public static class WebApplicationExtensions
{
    public static WebApplication MapCommonApiEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapGet("/", () => "Hello World!");

        app.UseAuthorization();
        app.MapHealthChecks("/health");
        app.MapControllers();

        return app;
    }
}
