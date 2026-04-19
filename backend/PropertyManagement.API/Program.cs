using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PropertyManagement.Application.Properties.Commands;
using PropertyManagement.API.Auth;
using PropertyManagement.Infrastructure;
using PropertyManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<DevAuthOptions>(builder.Configuration.GetSection(DevAuthOptions.SectionName));

builder.Services.AddControllers(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Property Management API",
        Version = "v1"
    });

    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter a Bearer token.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, bearerScheme);
});
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreatePropertyHandler).Assembly));
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

await WaitAndInitialize(app.Services, app.Logger);

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
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
