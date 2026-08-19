# ==========================================
# PersonaScript AI - Multi-stage Production Dockerfile (.NET 10)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar arquivos globais e solução
COPY ["Directory.Build.props", "./"]
COPY ["PersonaScript.slnx", "./"]

# Copiar arquivos de projeto (BuildingBlocks)
COPY ["src/BuildingBlocks/PersonaScript.BuildingBlocks.Domain/PersonaScript.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/PersonaScript.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/PersonaScript.BuildingBlocks.Results/PersonaScript.BuildingBlocks.Results.csproj", "src/BuildingBlocks/PersonaScript.BuildingBlocks.Results/"]
COPY ["src/BuildingBlocks/PersonaScript.BuildingBlocks.CQRS/PersonaScript.BuildingBlocks.CQRS.csproj", "src/BuildingBlocks/PersonaScript.BuildingBlocks.CQRS/"]
COPY ["src/BuildingBlocks/PersonaScript.BuildingBlocks.Tenancy/PersonaScript.BuildingBlocks.Tenancy.csproj", "src/BuildingBlocks/PersonaScript.BuildingBlocks.Tenancy/"]
COPY ["src/BuildingBlocks/PersonaScript.BuildingBlocks.AI/PersonaScript.BuildingBlocks.AI.csproj", "src/BuildingBlocks/PersonaScript.BuildingBlocks.AI/"]

# Copiar arquivos de projeto (Modules)
COPY ["src/Modules/Identity/PersonaScript.Modules.Identity.Domain/PersonaScript.Modules.Identity.Domain.csproj", "src/Modules/Identity/PersonaScript.Modules.Identity.Domain/"]
COPY ["src/Modules/Identity/PersonaScript.Modules.Identity.Application/PersonaScript.Modules.Identity.Application.csproj", "src/Modules/Identity/PersonaScript.Modules.Identity.Application/"]
COPY ["src/Modules/Identity/PersonaScript.Modules.Identity.Infrastructure/PersonaScript.Modules.Identity.Infrastructure.csproj", "src/Modules/Identity/PersonaScript.Modules.Identity.Infrastructure/"]

COPY ["src/Modules/Anamnese/PersonaScript.Modules.Anamnese.Domain/PersonaScript.Modules.Anamnese.Domain.csproj", "src/Modules/Anamnese/PersonaScript.Modules.Anamnese.Domain/"]
COPY ["src/Modules/Anamnese/PersonaScript.Modules.Anamnese.Application/PersonaScript.Modules.Anamnese.Application.csproj", "src/Modules/Anamnese/PersonaScript.Modules.Anamnese.Application/"]
COPY ["src/Modules/Anamnese/PersonaScript.Modules.Anamnese.Infrastructure/PersonaScript.Modules.Anamnese.Infrastructure.csproj", "src/Modules/Anamnese/PersonaScript.Modules.Anamnese.Infrastructure/"]

COPY ["src/Modules/Billing/PersonaScript.Modules.Billing.Domain/PersonaScript.Modules.Billing.Domain.csproj", "src/Modules/Billing/PersonaScript.Modules.Billing.Domain/"]
COPY ["src/Modules/Billing/PersonaScript.Modules.Billing.Application/PersonaScript.Modules.Billing.Application.csproj", "src/Modules/Billing/PersonaScript.Modules.Billing.Application/"]
COPY ["src/Modules/Billing/PersonaScript.Modules.Billing.Infrastructure/PersonaScript.Modules.Billing.Infrastructure.csproj", "src/Modules/Billing/PersonaScript.Modules.Billing.Infrastructure/"]

COPY ["src/Modules/Personas/PersonaScript.Modules.Personas.Domain/PersonaScript.Modules.Personas.Domain.csproj", "src/Modules/Personas/PersonaScript.Modules.Personas.Domain/"]
COPY ["src/Modules/Personas/PersonaScript.Modules.Personas.Application/PersonaScript.Modules.Personas.Application.csproj", "src/Modules/Personas/PersonaScript.Modules.Personas.Application/"]
COPY ["src/Modules/Personas/PersonaScript.Modules.Personas.Infrastructure/PersonaScript.Modules.Personas.Infrastructure.csproj", "src/Modules/Personas/PersonaScript.Modules.Personas.Infrastructure/"]

COPY ["src/Modules/Scripts/PersonaScript.Modules.Scripts.Domain/PersonaScript.Modules.Scripts.Domain.csproj", "src/Modules/Scripts/PersonaScript.Modules.Scripts.Domain/"]
COPY ["src/Modules/Scripts/PersonaScript.Modules.Scripts.Application/PersonaScript.Modules.Scripts.Application.csproj", "src/Modules/Scripts/PersonaScript.Modules.Scripts.Application/"]
COPY ["src/Modules/Scripts/PersonaScript.Modules.Scripts.Infrastructure/PersonaScript.Modules.Scripts.Infrastructure.csproj", "src/Modules/Scripts/PersonaScript.Modules.Scripts.Infrastructure/"]

# Copiar arquivos de projeto (Presentation)
COPY ["src/Presentation/PersonaScript.Server/PersonaScript.Server.csproj", "src/Presentation/PersonaScript.Server/"]

# Restaurar pacotes NuGet
RUN dotnet restore "src/Presentation/PersonaScript.Server/PersonaScript.Server.csproj"

# Copiar todo o código-fonte restante
COPY . .

# Compilar aplicação
WORKDIR "/src/src/Presentation/PersonaScript.Server"
RUN dotnet build "PersonaScript.Server.csproj" -c Release -o /app/build

# Publicar binários
FROM build AS publish
RUN dotnet publish "PersonaScript.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PersonaScript.Server.dll"]
