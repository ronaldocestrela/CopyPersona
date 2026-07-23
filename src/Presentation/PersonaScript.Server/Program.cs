using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Infrastructure;
using PersonaScript.Modules.Identity.Infrastructure;
using PersonaScript.Modules.Personas.Infrastructure;
using PersonaScript.Modules.Scripts.Infrastructure;
using PersonaScript.Server.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTenancy();
builder.Services.AddIdentityModule();
builder.Services.AddBillingModule();
builder.Services.AddPersonasModule();
builder.Services.AddScriptsModule();

builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program;
