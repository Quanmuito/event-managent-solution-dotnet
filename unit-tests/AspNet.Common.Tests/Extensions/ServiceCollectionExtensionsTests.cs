namespace AspNet.Common.Tests.Extensions;

using AspNet.Common.Extensions;
using AspNet.Common.Tests.Helpers;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Xunit;

public class ServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(typeof(Microsoft.Extensions.Options.IOptions<MvcOptions>))]
    [InlineData(typeof(Microsoft.AspNetCore.Mvc.ApiExplorer.IApiDescriptionGroupCollectionProvider))]
    [InlineData(typeof(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService))]
    [InlineData(typeof(Microsoft.AspNetCore.Http.IProblemDetailsService))]
    public void AddCommonApiServices_ShouldRegisterService(Type serviceType)
    {
        var services = AspNetCommonTestHelper.CreateServiceCollectionWithApiVersion(new ApiVersion(1, 0));
        services.AddCommonApiServices(new ApiVersion(1, 0));

        var serviceProvider = AspNetCommonTestHelper.BuildServiceProvider(services);
        var service = serviceProvider.GetService(serviceType);

        service.Should().NotBeNull();
    }

    [Fact]
    public void AddCommonApiServices_ShouldConfigureApiVersioning()
    {
        var defaultVersion = new ApiVersion(2, 1);
        var services = AspNetCommonTestHelper.CreateServiceCollectionWithApiVersion(defaultVersion);
        services.AddCommonApiServices(defaultVersion);

        var serviceProvider = AspNetCommonTestHelper.BuildServiceProvider(services);
        var apiVersioningOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Asp.Versioning.ApiVersioningOptions>>();

        apiVersioningOptions.Value.DefaultApiVersion.Should().Be(defaultVersion);
        apiVersioningOptions.Value.ReportApiVersions.Should().BeTrue();
        apiVersioningOptions.Value.AssumeDefaultVersionWhenUnspecified.Should().BeTrue();
    }

    [Fact]
    public void AddCommonApiServices_ShouldReturnServiceCollection()
    {
        var services = AspNetCommonTestHelper.CreateServiceCollectionWithApiVersion(new ApiVersion(1, 0));
        var defaultVersion = new ApiVersion(1, 0);

        var result = services.AddCommonApiServices(defaultVersion);

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddApiVersioningWithDefaults_ShouldConfigureReportApiVersions()
    {
        var services = AspNetCommonTestHelper.CreateServiceCollectionWithApiVersion(new ApiVersion(1, 0));
        services.AddApiVersioningWithDefaults(new ApiVersion(1, 0));

        var serviceProvider = AspNetCommonTestHelper.BuildServiceProvider(services);
        var apiVersioningOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Asp.Versioning.ApiVersioningOptions>>();

        apiVersioningOptions.Value.ReportApiVersions.Should().BeTrue();
    }

    [Fact]
    public void AddApiVersioningWithDefaults_ShouldConfigureAssumeDefaultVersionWhenUnspecified()
    {
        var services = AspNetCommonTestHelper.CreateServiceCollectionWithApiVersion(new ApiVersion(1, 0));
        services.AddApiVersioningWithDefaults(new ApiVersion(1, 0));

        var serviceProvider = AspNetCommonTestHelper.BuildServiceProvider(services);
        var apiVersioningOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Asp.Versioning.ApiVersioningOptions>>();

        apiVersioningOptions.Value.AssumeDefaultVersionWhenUnspecified.Should().BeTrue();
    }

    [Fact]
    public void AddApiVersioningWithDefaults_ShouldSetDefaultApiVersion()
    {
        var defaultVersion = new ApiVersion(2, 1);
        var services = AspNetCommonTestHelper.CreateServiceCollectionWithApiVersion(defaultVersion);
        services.AddApiVersioningWithDefaults(defaultVersion);

        var serviceProvider = AspNetCommonTestHelper.BuildServiceProvider(services);
        var apiVersioningOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Asp.Versioning.ApiVersioningOptions>>();

        apiVersioningOptions.Value.DefaultApiVersion.Should().Be(defaultVersion);
    }

    [Fact]
    public void AddApiVersioningWithDefaults_ShouldReturnServiceCollection()
    {
        var services = AspNetCommonTestHelper.CreateServiceCollectionWithApiVersion(new ApiVersion(1, 0));
        var defaultVersion = new ApiVersion(1, 0);

        var result = services.AddApiVersioningWithDefaults(defaultVersion);

        result.Should().BeSameAs(services);
    }
}
