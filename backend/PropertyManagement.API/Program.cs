using PropertyManagement.Application.Properties.Commands;
using PropertyManagement.Infrastructure;
using PropertyManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreatePropertyHandler).Assembly));
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

await WaitAndInitialize(app.Services, app.Logger);

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();
app.Run();


static async Task WaitAndInitialize(IServiceProvider services, ILogger logger)
{
    const int maxRetries = 20;

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            await DatabaseInitializer.InitializeAsync(services);
            return;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            logger.LogWarning("Database not ready (attempt {Attempt}/{Max}): {Msg}. Retrying in 5s...",
                attempt, maxRetries, ex.Message);
            await Task.Delay(5_000);
        }
    }
}
