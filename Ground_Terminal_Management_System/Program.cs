using Ground_Terminal_Management_System;
using Ground_Terminal_Management_System.Components;
using Ground_Terminal_Management_System.Hubs;
using Ground_Terminal_Management_System.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add SignalR
builder.Services.AddSignalR();

// Add service registrations

builder.Services.AddSingleton(provider =>
{
    int port = 5000; // Define your port
    return new TcpMessageReaderService(port);
});

// Register Razor Components and enable interactive server components.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


// Register a hosted service to run TcpMessageReaderService in the background.
builder.Services.AddHostedService<TcpMessageReaderBackgroundService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=localhost;Database=DataVisualization;User Id=sa;Password=17039125Ss#;Encrypt=True;TrustServerCertificate=True;"));


// Build the application.
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
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
