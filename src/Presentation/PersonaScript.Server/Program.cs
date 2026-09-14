using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PersonaScript.BuildingBlocks.AI;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Infrastructure;
using PersonaScript.Modules.Backoffice;
using PersonaScript.Modules.Billing.Infrastructure;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;
using PersonaScript.Modules.Identity.Infrastructure;
using PersonaScript.Modules.Personas.Infrastructure;
using PersonaScript.Modules.Scripts.Infrastructure;
using PersonaScript.Server.Components;
using PersonaScript.Server.Endpoints;
using PersonaScript.Server.Middleware;
using PersonaScript.Server.Observability;
using Serilog;

// Carrega as variáveis de ambiente a partir do arquivo .env (se existir)
Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProcessId()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithProperty("Application", "PersonaScript.Server");

    if (context.HostingEnvironment.IsDevelopment())
    {
        configuration.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{TenantId}] {Message:lj}{NewLine}{Exception}");
    }
    else
    {
        configuration.WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter());
    }

    var seqUrl = context.Configuration["Seq:ServerUrl"] ?? context.Configuration["SERILOG__WRITETO__SEQ__SERVERURL"];
    if (!string.IsNullOrWhiteSpace(seqUrl))
    {
        configuration.WriteTo.Seq(seqUrl);
    }
});

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.UseStaticWebAssets();
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var jwtKey = Encoding.UTF8.GetBytes(jwtOptions.Secret);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/acesso-negado";
        options.LogoutPath = "/logout";
        options.Cookie.Name = "PersonaScript.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = true;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
        };
    });

builder.Services.AddAuthorization(options =>
{
    var defaultAuthSchemes = new[] { CookieAuthenticationDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme };

    options.AddPolicy("RequireSystemAdmin", policy =>
        policy.AddAuthenticationSchemes(defaultAuthSchemes)
              .RequireRole(UserRole.SystemAdmin.ToString()));

    options.AddPolicy("RequireSupportAgent", policy =>
        policy.AddAuthenticationSchemes(defaultAuthSchemes)
              .RequireRole(UserRole.SupportAgent.ToString(), UserRole.SystemAdmin.ToString()));

    options.AddPolicy("RequireFinanceAdmin", policy =>
        policy.AddAuthenticationSchemes(defaultAuthSchemes)
              .RequireRole(UserRole.FinanceAdmin.ToString(), UserRole.SystemAdmin.ToString()));

    options.AddPolicy("RequireBackofficeAccess", policy =>
        policy.AddAuthenticationSchemes(defaultAuthSchemes)
              .RequireRole(
                  UserRole.SupportAgent.ToString(),
                  UserRole.FinanceAdmin.ToString(),
                  UserRole.SystemAdmin.ToString()));
});
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddTenancy();
builder.Services.AddAIBuildingBlock(builder.Configuration);
builder.Services.AddIdentityModule(builder.Configuration, builder.Environment);
builder.Services.AddAnamneseModule(builder.Configuration);
builder.Services.AddBillingModule(builder.Configuration);
builder.Services.AddPersonasModule(builder.Configuration);
builder.Services.AddScriptsModule(builder.Configuration);
builder.Services.AddBackofficeModule(builder.Configuration);
builder.Services.AddScoped<PersonaScript.Server.Services.IQuotaNotifierService, PersonaScript.Server.Services.QuotaNotifierService>();
builder.Services.AddScoped<IImpersonationService, PersonaScript.Server.Services.CookieImpersonationService>();
builder.Services.AddSecurityRateLimiting(builder.Configuration);

builder.Services.AddPersonaScriptObservability(builder.Configuration);

var app = builder.Build();

var applyMigrations = app.Environment.IsDevelopment() ||
                      app.Configuration.GetValue<bool>("APPLY_MIGRATIONS") ||
                      app.Configuration.GetValue<bool>("ApplyMigrationsOnStartup");

if (applyMigrations)
{
    await app.Services.ApplyIdentityMigrationsAsync();
    await app.Services.ApplyAnamneseMigrationsAsync();
    await app.Services.ApplyBillingMigrationsAsync();
    await app.Services.ApplyPersonasMigrationsAsync();
    await app.Services.ApplyScriptsMigrationsAsync();
    await app.Services.ApplyBackofficeMigrationsAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api") && !context.Request.Path.StartsWithSegments("/webhooks"),
    appBuilder => appBuilder.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));

app.UseMiddleware<PersonaScript.Server.Middleware.SecurityHeadersMiddleware>();
app.UseMiddleware<PersonaScript.Server.Middleware.PerformanceTimingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseTenantLogContext();
app.UseAntiforgery();
app.UseRateLimiter();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPersonaScriptHealthEndpoints();
app.MapAccountEndpoints();
app.MapBackofficeEndpoints();
app.MapStripeEndpoints();
app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.Run();

public partial class Program;
