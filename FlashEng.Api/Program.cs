using FlashEng.Api.Middleware;
using FlashEng.Bll.Interfaces;
using FlashEng.Bll.Services;
using FlashEng.Bll.Mapping;
using FlashEng.Dal.Interfaces;
using FlashEng.Dal.UnitOfWork;
using FlashEng.Dal.Configuration;
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

// Connection strings
var usersConnectionString = configuration.GetConnectionString("UsersConnection")!;
var flashcardsConnectionString = configuration.GetConnectionString("FlashcardsConnection")!;
var ordersConnectionString = configuration.GetConnectionString("OrdersConnection")!;
var serverConnectionString = configuration.GetConnectionString("ServerConnection")!;

// Реєстрація сервісів
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FlashEng API",
        Version = "v1",
        Description = "FlashEng API with three-layer architecture (DAL → BLL → API)"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork>(provider =>
    new UnitOfWork(usersConnectionString, flashcardsConnectionString, ordersConnectionString));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Ініціалізація бази даних
try
{
    Log.Information("Initializing databases...");

    await DatabaseConfig.EnsureDatabasesCreatedAsync(serverConnectionString);
    await DatabaseConfig.CreateUsersTablesAsync(usersConnectionString);
    await DatabaseConfig.CreateFlashcardsTablesAsync(flashcardsConnectionString);
    await DatabaseConfig.CreateOrdersTablesAsync(ordersConnectionString);

    Log.Information("Databases initialized successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An error occurred while initializing databases");
    throw;
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlashEng API v1");
        c.RoutePrefix = "swagger"; // Swagger доступний на /swagger
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

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