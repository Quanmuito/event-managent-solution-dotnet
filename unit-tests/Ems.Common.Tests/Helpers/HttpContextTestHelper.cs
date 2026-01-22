namespace Ems.Common.Tests.Helpers;

using System.Text.Json;
using Microsoft.AspNetCore.Http;

public static class HttpContextTestHelper
{
    public static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    public static async Task<T> GetResponseBody<T>(HttpContext httpContext)
    {
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(httpContext.Response.Body);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
