using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Personas.Application.Commands.GeneratePersonaDiagnosis;
using PersonaScript.Modules.Personas.Application.Commands.UpdatePersonaDiagnosis;
using PersonaScript.Modules.Personas.Application.DTOs;
using PersonaScript.Modules.Personas.Application.Queries.GetPersonaDiagnosis;
using PersonaScript.Modules.Personas.Application.Services;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Domain.ValueObjects;
using PersonaScript.Modules.Personas.Infrastructure.Persistence;
using PersonaScript.Modules.Personas.Infrastructure.Repositories;
using Xunit;

namespace PersonaScript.Modules.Personas.Tests.Security;

public class PersonaCrossTenantSecurityTests
{
    private static PersonasDbContext CreateDbContext(string dbName, ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<PersonasDbContext>()
            .UseInMemoryDatabase(dbName)
            .AddInterceptors(new TenantDbContextInterceptor(tenantContext))
            .Options;

        return new PersonasDbContext(options, tenantContext);
    }

    private static PersonaDiagnosis CreateDiagnosis(Guid tenantId, string frase)
    {
        return PersonaDiagnosis.Create(
            tenantId,
            Guid.NewGuid(),
            frase,
            "Síntese do perfil",
            new IdentidadeMarca("Tom Profissional", "Estilo Clean", "Especialista", "Mentor"),
            new List<PilarConteudo> { new PilarConteudo("Educação Médica", 100, "Desc", new[] { "Prevenção" }) },
            new MatrizRestricoes(new[] { "Preços" }, new[] { "Garantia" }, new[] { "CFM 2.336" }, "Sem antes e depois sensacionalista")
        ).Value!;
    }

    [Fact]
    public async Task Repository_GetByIdAsync_WhenCalledByForeignTenant_ShouldReturnNull()
    {
        // Arrange
        var dbName = "PersonaSecurity_" + Guid.NewGuid();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
        var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));

        Guid diagnosisIdA;

        // Tenant A salva seu diagnóstico
        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var repoA = new PersonaDiagnosisRepository(contextA);
            var diagnosisA = CreateDiagnosis(tenantA, "Posicionamento Dra. Ana");
            await repoA.AddAsync(diagnosisA);
            await repoA.SaveChangesAsync();
            diagnosisIdA = diagnosisA.Id;
        }

        // Act - Tenant B tenta consultar o ID direto de Tenant A
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var repoB = new PersonaDiagnosisRepository(contextB);
            var resultB = await repoB.GetByIdAsync(diagnosisIdA);

            // Assert: Query filter isola e retorna null
            resultB.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetPersonaDiagnosisQuery_TenantBNaoDeveVerDiagnosticoDoTenantA()
    {
        // Arrange
        var dbName = "PersonaSecurity_" + Guid.NewGuid();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
        var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));

        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var repoA = new PersonaDiagnosisRepository(contextA);
            await repoA.AddAsync(CreateDiagnosis(tenantA, "Posicionamento Tenant A"));
            await repoA.SaveChangesAsync();
        }

        // Act - Tenant B executa a query
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var repoB = new PersonaDiagnosisRepository(contextB);
            var handlerB = new GetPersonaDiagnosisQueryHandler(repoB, tenantContextB);

            var queryResult = await handlerB.Handle(new GetPersonaDiagnosisQuery(), CancellationToken.None);

            // Assert: Sucesso porém Value é nulo (nenhum diagnóstico para Tenant B)
            queryResult.IsSuccess.Should().BeTrue();
            queryResult.Value.Should().BeNull();
        }
    }

    [Fact]
    public async Task GeneratePersonaDiagnosisCommand_TenantBNaoDeveGerarSemAnamnesePropria()
    {
        // Arrange
        var dbName = "PersonaSecurity_" + Guid.NewGuid();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));

        // Mock GetFullAnamneseQueryHandler simulando que Tenant B não tem anamnese (retorna Failure)
        var anamneseQueryHandlerMock = Substitute.For<IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto>>();
        anamneseQueryHandlerMock.Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<FullAnamneseDto>(PersonaScript.Modules.Personas.Domain.DomainErrors.Personas.AnamneseNaoEncontrada));

        var generatorMock = Substitute.For<IPersonaDiagnosisGenerator>();

        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var repoB = new PersonaDiagnosisRepository(contextB);
            var handlerB = new GeneratePersonaDiagnosisCommandHandler(repoB, tenantContextB, anamneseQueryHandlerMock, generatorMock);

            // Act
            var result = await handlerB.Handle(new GeneratePersonaDiagnosisCommand(), CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(PersonaScript.Modules.Personas.Domain.DomainErrors.Personas.AnamneseNaoEncontrada);
        }
    }

    [Fact]
    public async Task UpdatePersonaDiagnosisCommand_TenantBNaoDeveAlterarDiagnosticoDoTenantA()
    {
        // Arrange
        var dbName = "PersonaSecurity_" + Guid.NewGuid();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
        var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));

        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var repoA = new PersonaDiagnosisRepository(contextA);
            await repoA.AddAsync(CreateDiagnosis(tenantA, "Posicionamento Original do Tenant A"));
            await repoA.SaveChangesAsync();
        }

        // Tenant B tenta executar Update
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var repoB = new PersonaDiagnosisRepository(contextB);
            var handlerB = new UpdatePersonaDiagnosisCommandHandler(repoB, tenantContextB);

            var updateCmd = new UpdatePersonaDiagnosisCommand(
                "Posicionamento Hacker",
                "Síntese Maliciosa",
                new IdentidadeMarcaDto("Agressivo", "Escuro", "Rebelde", string.Empty),
                new List<PilarConteudoDto> { new PilarConteudoDto("Spam", 100, "Desc", new List<string>()) },
                new MatrizRestricoesDto(new List<string>(), new List<string>(), new List<string>(), "Nenhum")
            );

            var result = await handlerB.Handle(updateCmd, CancellationToken.None);

            // Assert: Tenant B não encontra diagnóstico
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(PersonaScript.Modules.Personas.Domain.DomainErrors.Personas.DiagnosticoNaoEncontrado);
        }

        // Valida que o diagnóstico de Tenant A permanece intacto
        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var repoA = new PersonaDiagnosisRepository(contextA);
            var diagnosisA = await repoA.GetByTenantIdAsync();

            diagnosisA.Should().NotBeNull();
            diagnosisA!.FrasePosicionamento.Should().Be("Posicionamento Original do Tenant A");
        }
    }
}
