using FlashEng.Api.Middleware;
using FlashEng.Bll.Interfaces;
using FlashEng.Bll.Services;
using FlashEng.Bll.Mapping;
using FlashEng.Dal.Interfaces;
using FlashEng.Dal.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Налаштування Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Конфігурація
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection")!;

// Реєстрація сервісів
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FlashEng API",
        Version = "v1",
        Description = "FlashEng API with EF Core and three-layer architecture"
    });
});

// Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork>(provider =>
{
    var context = provider.GetRequiredService<AppDbContext>();
    return new FlashEng.Dal.UnitOfWork.UnitOfWork(context);
});
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Міграції та Seeding
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (app.Environment.IsDevelopment())
    {
        // В Development завжди пересіджуємо базу
        Log.Information("Development mode: Recreating database...");
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        Log.Information("Seeding fresh data...");
        await FlashEng.Dal.Seeding.DatabaseSeeder.SeedAsync(context);

        Log.Information("Database recreated and seeded successfully");
    }
    else
    {
        // В Production тільки міграції
        Log.Information("Production mode: Applying migrations...");
        await context.Database.MigrateAsync();

        // Сід тільки якщо база порожня
        if (!await context.Users.AnyAsync())
        {
            Log.Information("Seeding initial data...");
            await FlashEng.Dal.Seeding.DatabaseSeeder.SeedAsync(context);
        }

        Log.Information("Database initialized successfully");
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "An error occurred while initializing the database");
    throw;
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlashEng API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

try
{
    Log.Information("Starting FlashEng API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}