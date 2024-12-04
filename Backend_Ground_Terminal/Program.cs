/*
* FILE : Program.cs
* PROJECT : SENG3020 - Milestone #2 
* PROGRAMMER : Shivang Chordia, Keval PAtel, Urvish Motivaras & Jaygiri Goswami
* DATE : 2024-11-22
* DESCRIPTION : This file sets up a .NET Core Web API application for managing a ground terminal system. It configures services, middleware, and routing to support telemetry data handling and
*               real-time communication
*/

using Backend_Ground_Terminal.Hubs;
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
    var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<DatabaseService>();
    DatabaseService.Initialize(configuration, logger);  // Initialize DatabaseService with configuration

    // Ensure DatabaseService is not nullable when used
    return DatabaseService.Instance ?? throw new InvalidOperationException("DatabaseService instance is not initialized.");
});


builder.Services.AddSingleton<TcpMessageReaderService>(provider =>
{
    var databaseService = provider.GetRequiredService<DatabaseService>();
    var hubContext = provider.GetRequiredService<IHubContext<TelemetryHub>>();

    return new TcpMessageReaderService(12000, databaseService, hubContext);
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
