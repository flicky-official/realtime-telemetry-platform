using Npgsql;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Read the connection URL supplied through the environment.
var databaseUrl = builder.Configuration["DATABASE_URL"];

if (string.IsNullOrWhiteSpace(databaseUrl))
{
    throw new InvalidOperationException(
        "DATABASE_URL is missing. Load it into the environment before starting the API.");
}

var uri = new Uri(databaseUrl);
var credentials = uri.UserInfo.Split(':', 2);

if (credentials.Length != 2)
{
    throw new InvalidOperationException(
        "DATABASE_URL must contain a username and password.");
}

var connectionString = new NpgsqlConnectionStringBuilder
{
    Host = uri.Host,
    Port = uri.IsDefaultPort ? 5432 : uri.Port,
    Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
    Username = Uri.UnescapeDataString(credentials[0]),
    Password = Uri.UnescapeDataString(credentials[1])
};

// One shared data source manages the database connection pool.
builder.Services.AddSingleton<NpgsqlDataSource>(_ =>
    NpgsqlDataSource.Create(connectionString.ConnectionString));

builder.Services.AddDbContext<AppDbContext>((services, options) =>
    options.UseNpgsql(
        services.GetRequiredService<NpgsqlDataSource>()));

builder.Services.AddHostedService<MqttTelemetrySubscriber>();
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "MyApp API",
    status = "running"
}));

app.MapGet("/health/db", async (
    NpgsqlDataSource dataSource,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        await using var command = dataSource.CreateCommand("SELECT 1");
        var result = await command.ExecuteScalarAsync(cancellationToken);

        return Results.Ok(new
        {
            database = "connected",
            queryResult = Convert.ToInt32(result)
        });
    }
    catch (NpgsqlException exception)
    {
        logger.LogError(exception, "Database health check failed.");

        return Results.Problem(
            title: "Database unavailable",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapGet("/devices/{deviceId}/latest", async Task<IResult> (
    string deviceId,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var device = await db.Devices
        .AsNoTracking()
        .Where(d => d.Id == deviceId)
        .Select(d => new
        {
            d.Id,
            d.Name
        })
        .SingleOrDefaultAsync(cancellationToken);

    if (device is null)
    {
        return Results.NotFound(new
        {
            message = "Device not found."
        });
    }

    var latest = await db.TelemetrySamples
        .AsNoTracking()
        .Where(s => s.DeviceId == deviceId)
        .OrderByDescending(s => s.RecordedAtUtc)
        .ThenByDescending(s => s.Id)
        .Select(s => new
        {
            s.Id,
            s.RecordedAtUtc,
            s.ReceivedAtUtc,
            s.CpuTemperatureC,
            s.CpuUsagePercent,
            s.CpuClockMhz,
            s.MemoryUsedBytes,
            s.MemoryTotalBytes,
            s.IntegratedGpuTemperatureC,
            s.IntegratedGpuClockMhz,
            s.NvidiaGpuTemperatureC,
            s.NvidiaGpuUsagePercent,
            s.NvidiaGpuClockMhz,
            s.NvidiaMemoryUsedBytes,
            s.NvidiaMemoryTotalBytes,
            s.CpuFanRpm,
            s.GpuFanRpm,
            s.SsdTemperatureC
        })
        .FirstOrDefaultAsync(cancellationToken);

    return Results.Ok(new
    {
        device,
        latest
    });
});

app.Run();
