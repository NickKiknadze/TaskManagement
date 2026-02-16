using Tasky.Application;
using Tasky.Infrastructure;
using Tasky.Infrastructure.Persistence;
using Orleans.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionCorsPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetValue<string>("Cors:AllowedOrigins")?.Split(',') ?? ["http://localhost:5173"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Host.UseOrleans(silo =>
    {
        var clusteringProvider = builder.Configuration.GetValue<string>("Orleans:ClusteringProvider") ?? "Localhost";
        
        if (clusteringProvider == "Localhost")
        {
            silo.UseLocalhostClustering();
        }
        else
        {
        }
        
        silo.Services.AddSerializer(serializerBuilder =>
        {
            serializerBuilder.AddJsonSerializer(isSupported: type => 
                type.Namespace != null && type.Namespace.StartsWith("Tasky.Domain"));
        });
    });
}

builder.Services.AddSignalR();
builder.Services.AddScoped<Tasky.Application.Interfaces.ISignalRNotifier, Tasky.Api.Services.SignalRNotifier>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    await next();
});

app.UseCors("ProductionCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapHub<Tasky.Api.Hubs.TaskHub>("/hubs/realtime");

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing") || app.Configuration.GetValue<bool>("Seeding:Enabled")) 
{
    using (var scope = app.Services.CreateScope())
    {
        var initialiser = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var defaultPassword = config["DefaultAdminPassword"] ?? "Aa!12345#"; 
        await initialiser.InitializeAsync(defaultPassword);
    }
}

app.Run();

public partial class Program { }
