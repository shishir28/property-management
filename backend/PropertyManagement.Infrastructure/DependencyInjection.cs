using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropertyManagement.Application.Common;
using PropertyManagement.Domain.Finance;
using PropertyManagement.Domain.Inspections;
using PropertyManagement.Domain.Leases;
using PropertyManagement.Domain.Maintenance;
using PropertyManagement.Domain.Properties;
using PropertyManagement.Domain.Tenants;
using PropertyManagement.Domain.Workflows;
using PropertyManagement.Infrastructure.Persistence;
using PropertyManagement.Infrastructure.Persistence.Repositories;
using InspectionRepository = PropertyManagement.Infrastructure.Persistence.Repositories.InspectionRepository;
using PropertyManagement.Infrastructure.Workflows;

namespace PropertyManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("TiDb")
            ?? throw new InvalidOperationException("Connection string 'TiDb' is missing.");

        services.AddDbContext<AppDbContext>(opts =>
            opts.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ILeaseRepository, LeaseRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IInspectionRepository, InspectionRepository>();

        services.AddHttpClient<IWorkflowClient, WorkflowClient>(client =>
        {
            var baseUrl = config["Orchestration:BaseUrl"] ?? "http://localhost:8000";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
