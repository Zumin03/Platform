using InstrumentPlatform.Data;
using InstrumentPlatform.Extensions;
using InstrumentPlatform.Handlers;
using InstrumentPlatform.Options;
using InstrumentPlatform.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IMeasurementService, MeasurementService>();
builder.Services.AddScoped<IInstrumentService, InstrumentService>();
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<ISerialCommunicationService, SerialCommunicationService>();
builder.Services.AddScoped<IInstrumentErrorHandler, InstrumentErrorHandler>();
builder.Services.AddScoped<ITimeService, TimeService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<TimeZoneOptions>(
    builder.Configuration.GetSection("TimeZone"));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

var scope = app.Services.CreateScope();

// Check database and aplly migration if needed.
app.ApplyMigration();

// Reset instrument states in database to default values.
var detectionService = scope.ServiceProvider.GetRequiredService<IInstrumentService>();
await detectionService.DetectInstrumentsAsync();


app.Run();
