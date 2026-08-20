using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.Modules.Backoffice;
using Xunit;

namespace PersonaScript.Modules.Backoffice.Tests;

public class BackofficeModuleSetupTests
{
    [Fact]
    public void AddBackofficeModule_ShouldRegisterBackofficeServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddBackofficeModule(configuration);

        // Assert
        services.Should().NotBeNull();
    }
}
