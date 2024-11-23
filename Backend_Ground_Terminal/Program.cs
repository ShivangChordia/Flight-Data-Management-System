using Backend_Ground_Terminal;
using Ground_Terminal_Management_System.Services;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSignalR();

builder.Services.AddControllers();
// Add Swagger configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Register DatabaseService and initialize it with IConfiguration
builder.Services.AddSingleton(provider =>
{
    var configuration = builder.Configuration; // Access IConfiguration
    DatabaseService.Initialize(configuration);  // Initialize DatabaseService with configuration
    return DatabaseService.Instance; // Return the singleton instance
});
builder.Services.AddSingleton<TcpMessageReaderService>(provider =>
{
    var databaseService = provider.GetRequiredService<DatabaseService>();
    var hubContext = provider.GetRequiredService<IHubContext<TelemetryHub>>();

    return new TcpMessageReaderService(5000, databaseService, hubContext);
});
// Register a hosted service to run TcpMessageReaderService in the background.
builder.Services.AddHostedService<TcpMessageReaderBackgroundService>();


var app = builder.Build();

// Enable middleware for Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();  // Enable Swagger middleware
    app.UseSwaggerUI();
}

app.MapHub<TelemetryHub>("/telemetryHub");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();