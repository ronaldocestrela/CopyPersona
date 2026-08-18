using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.BuildingBlocks.Domain;

namespace PersonaScript.BuildingBlocks.Tenancy;

public static class ModelBuilderTenantExtensions
{
    public static void ApplyTenantQueryFilters<TContext>(this ModelBuilder modelBuilder, TContext dbContext)
        where TContext : DbContext
    {
        var tenantContextMember = GetTenantContextMember(typeof(TContext));
        if (tenantContextMember is null)
        {
            return;
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var method = typeof(ModelBuilderTenantExtensions)
                .GetMethod(nameof(SetTenantFilterForContext), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(TContext), entityType.ClrType);

            method.Invoke(null, [modelBuilder, dbContext, tenantContextMember]);
        }
    }

    public static void ApplyTenantQueryFilters(this ModelBuilder modelBuilder, ITenantContext tenantContext)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var method = typeof(ModelBuilderTenantExtensions)
                .GetMethod(nameof(SetTenantFilterDirect), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(null, [modelBuilder, tenantContext]);
        }
    }

    private static void SetTenantFilterForContext<TContext, TEntity>(
        ModelBuilder modelBuilder,
        TContext dbContext,
        MemberInfo tenantContextMember)
        where TContext : DbContext
        where TEntity : class, IMustHaveTenant
    {
        var entityParam = Expression.Parameter(typeof(TEntity), "e");
        var dbContextConst = Expression.Constant(dbContext);
        
        var tenantContextAccess = Expression.MakeMemberAccess(dbContextConst, tenantContextMember);
        var tenantIdAccess = Expression.Property(tenantContextAccess, nameof(ITenantContext.TenantId));
        var tenantGuidAccess = Expression.Property(tenantIdAccess, nameof(TenantId.Value));
        
        var entityTenantIdAccess = Expression.Property(entityParam, nameof(IMustHaveTenant.TenantId));
        var equalsExpr = Expression.Equal(entityTenantIdAccess, tenantGuidAccess);

        var lambda = Expression.Lambda<Func<TEntity, bool>>(equalsExpr, entityParam);
        modelBuilder.Entity<TEntity>().HasQueryFilter(lambda);
    }

    private static void SetTenantFilterDirect<TEntity>(ModelBuilder modelBuilder, ITenantContext tenantContext)
        where TEntity : class, IMustHaveTenant
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity => entity.TenantId == tenantContext.TenantId.Value);
    }

    private static MemberInfo? GetTenantContextMember(Type contextType)
    {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var prop = contextType.GetProperties(flags).FirstOrDefault(p => typeof(ITenantContext).IsAssignableFrom(p.PropertyType));
        if (prop is not null)
        {
            return prop;
        }

        return contextType.GetFields(flags).FirstOrDefault(f => typeof(ITenantContext).IsAssignableFrom(f.FieldType));
    }
}

public static class TenancyServiceCollectionExtensions
{
    public static IServiceCollection AddTenancy(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, HttpContextTenantContext>();
        services.AddScoped<TenantDbContextInterceptor>();
        return services;
    }
}
