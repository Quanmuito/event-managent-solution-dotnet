namespace AspNet.Common.Tests.Extensions;

using AspNet.Common.Extensions;
using AspNet.Common.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentAssertions;
using Xunit;

public class WebApplicationExtensionsTests
{
    [Fact]
    public void MapCommonApiEndpoints_ShouldReturnWebApplication()
    {
        var builder = AspNetCommonTestHelper.CreateWebApplicationBuilder(Environments.Development);
        builder.Services.AddControllers();
        builder.Services.AddHealthChecks();
        var app = AspNetCommonTestHelper.BuildWebApplication(builder);

        var result = app.MapCommonApiEndpoints();

        result.Should().BeSameAs(app);
    }
}
