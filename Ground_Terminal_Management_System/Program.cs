using Ground_Terminal_Management_System;
using Ground_Terminal_Management_System.Components;
using Ground_Terminal_Management_System.Hubs;
using Ground_Terminal_Management_System.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add SignalR
builder.Services.AddSignalR();

// Register TcpMessageReaderService and inject DatabaseService
builder.Services.AddSingleton<TcpMessageReaderService>(provider =>
{
    var databaseService = provider.GetRequiredService<DatabaseService>();
    return new TcpMessageReaderService(5000, databaseService); // Inject it into TcpMessageReaderService
});

// Register Razor Components and enable interactive server components.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register a hosted service to run TcpMessageReaderService in the background.
builder.Services.AddHostedService<TcpMessageReaderBackgroundService>();

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register DatabaseService and initialize it with IConfiguration
builder.Services.AddSingleton(provider =>
{
    var configuration = builder.Configuration; // Access IConfiguration
    DatabaseService.Initialize(configuration);  // Initialize DatabaseService with configuration
    return DatabaseService.Instance; // Return the singleton instance
});

// Build the application.
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Map Razor components to the App component.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map SignalR Hubs
app.MapHub<TelemetryHub>("/telemetryHub");

app.Run();
