using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.BuildingBlocks.Domain;

namespace PersonaScript.BuildingBlocks.Tenancy;

public static class ModelBuilderTenantExtensions
{
    public static void ApplyTenantQueryFilters(this ModelBuilder modelBuilder, ITenantContext tenantContext)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var method = typeof(ModelBuilderTenantExtensions)
                .GetMethod(nameof(SetTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(null, [modelBuilder, tenantContext]);
        }
    }

    private static void SetTenantFilter<TEntity>(ModelBuilder modelBuilder, ITenantContext tenantContext)
        where TEntity : class, IMustHaveTenant
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity => entity.TenantId == tenantContext.TenantId.Value);
    }
}

public static class TenancyServiceCollectionExtensions
{
    public static IServiceCollection AddTenancy(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, HttpContextTenantContext>();
        return services;
    }
}
