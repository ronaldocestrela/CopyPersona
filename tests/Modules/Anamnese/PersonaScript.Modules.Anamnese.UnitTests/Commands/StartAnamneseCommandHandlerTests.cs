using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.Commands.StartAnamnese;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;
using PersonaScript.Modules.Anamnese.Infrastructure.Repositories;
using Xunit;

namespace PersonaScript.Modules.Anamnese.UnitTests.Commands;

public class StartAnamneseCommandHandlerTests
{
    private static (AnamneseDbContext DbContext, AnamneseRepository Repository) CreateContextAndRepo(Guid tenantId, string dbName)
    {
        ITenantContext tenantContext = new FixedTenantContext(TenantId.From(tenantId));
        var options = new DbContextOptionsBuilder<AnamneseDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var dbContext = new AnamneseDbContext(options, tenantContext);
        var repo = new AnamneseRepository(dbContext);
        return (dbContext, repo);
    }

    [Fact]
    public async Task Handle_ComTenantValido_DeveCriarRascunhoESalvar()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();
        var tenantContext = new FixedTenantContext(TenantId.From(tenantId));

        using (var dbContext = CreateContextAndRepo(tenantId, dbName).DbContext)
        {
            var repo = new AnamneseRepository(dbContext);
            var handler = new StartAnamneseCommandHandler(repo, tenantContext);

            // Act
            var result = await handler.Handle(new StartAnamneseCommand(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();

            var saved = await repo.GetByTenantIdAsync();
            saved.Should().NotBeNull();
            saved!.Status.Should().Be(AnamneseStatus.Rascunho);
            saved.EtapaAtual.Should().Be(1);
            saved.PercentualConclusao.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_ComRascunhoExistente_DeveRetornarIdExistenteIdempotente()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();
        var tenantContext = new FixedTenantContext(TenantId.From(tenantId));

        Guid firstId;
        using (var dbContext = CreateContextAndRepo(tenantId, dbName).DbContext)
        {
            var repo = new AnamneseRepository(dbContext);
            var handler = new StartAnamneseCommandHandler(repo, tenantContext);
            var firstResult = await handler.Handle(new StartAnamneseCommand(), CancellationToken.None);
            firstId = firstResult.Value;
        }

        using (var dbContext = CreateContextAndRepo(tenantId, dbName).DbContext)
        {
            var repo = new AnamneseRepository(dbContext);
            var handler = new StartAnamneseCommandHandler(repo, tenantContext);

            // Act
            var secondResult = await handler.Handle(new StartAnamneseCommand(), CancellationToken.None);

            // Assert
            secondResult.IsSuccess.Should().BeTrue();
            secondResult.Value.Should().Be(firstId);
        }
    }

    [Fact]
    public async Task Handle_ComTenantVazio_DeveRetornarErroTenantIdInvalido()
    {
        // Arrange
        var tenantContext = new NullTenantContext();
        var options = new DbContextOptionsBuilder<AnamneseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var dbContext = new AnamneseDbContext(options, tenantContext);
        var repo = new AnamneseRepository(dbContext);
        var handler = new StartAnamneseCommandHandler(repo, tenantContext);

        // Act
        var result = await handler.Handle(new StartAnamneseCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Anamnese.TenantIdInvalido);
    }
}
