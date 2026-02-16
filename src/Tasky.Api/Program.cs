using Tasky.Application;
using Tasky.Infrastructure;
using Tasky.Infrastructure.Persistence;
using Orleans.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Auth
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection(Tasky.Infrastructure.Authentication.JwtOptions.SectionName).Get<Tasky.Infrastructure.Authentication.JwtOptions>();
        if (jwtSettings == null) return;
        
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
        
        // SignalR Auth query string support
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
             OnMessageReceived = context =>
             {
                 var accessToken = context.Request.Query["access_token"];
                 var path = context.HttpContext.Request.Path;
                 if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                 {
                     context.Token = accessToken;
                 }
                 return Task.CompletedTask;
             }
        };
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Tasky.Domain.Constants.Permissions.All())
    {
        options.AddPolicy(permission, policy => policy.RequireClaim("permissions", permission));
    }
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Tasky.Application.Interfaces.ICurrentUserService, Tasky.Api.Services.CurrentUserService>();

// Orleans
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Host.UseOrleans(silo =>
    {
        silo.UseLocalhostClustering();
        silo.Services.AddSerializer(serializerBuilder =>
        {
            serializerBuilder.AddJsonSerializer(isSupported: type => 
                type.Namespace != null && type.Namespace.StartsWith("Tasky.Domain"));
        });
    });
}

// SignalR
builder.Services.AddSignalR();
builder.Services.AddScoped<Tasky.Application.Interfaces.ISignalRNotifier, Tasky.Api.Services.SignalRNotifier>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapHub<Tasky.Api.Hubs.TaskHub>("/hubs/realtime");

// Seeding
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing") || app.Configuration.GetValue<bool>("Seeding:Enabled")) 
{
    using (var scope = app.Services.CreateScope())
    {
        var initialiser = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var defaultPassword = config["DefaultAdminPassword"] ?? "UpdatedPassw0rd!"; // Fallback
        await initialiser.InitializeAsync(defaultPassword);
    }
}

app.Run();

public partial class Program { }
