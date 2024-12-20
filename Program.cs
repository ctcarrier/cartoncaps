using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using CartonCapsApi.Services;
using CartonCapsApi.Data;
using CartonCapsApi.Middleware;
using Serilog;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Serilog config remains the same
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "{ \"Timestamp\":\"{Timestamp:O}\", \"Level\":\"{Level}\", \"Message\":\"{Message}\", \"Properties\":{Properties}, \"Exception\":\"{Exception}\"}\n"
        );
});

builder.Services.AddControllers();

// EF InMemory
builder.Services.AddDbContext<ReferralContext>(options =>
    options.UseInMemoryDatabase("CartonCapsDb"));

// JWT with user-jwts defaults
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer <token>'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        { securityScheme, new string[] {} }
    });
});

// Services
builder.Services.AddScoped<IUserExternalService, MockUserExternalService>();
builder.Services.AddScoped<IReferralService, ReferralService>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate = "Handled {RequestPath}";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ReferralContext>();
    context.ReferredUsers.Add(new CartonCapsApi.Data.Entities.ReferredUser
    {
        UserId = "user123",
        ReferredUserId = "refUser1",
        CreatedDate = DateTime.UtcNow
    });
    context.ReferredUsers.Add(new CartonCapsApi.Data.Entities.ReferredUser
    {
        UserId = "user123",
        ReferredUserId = "refUser2",
        CreatedDate = DateTime.UtcNow
    });
    await context.SaveChangesAsync();
}

app.Run();
