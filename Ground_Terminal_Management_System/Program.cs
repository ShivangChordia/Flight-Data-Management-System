using Ground_Terminal_Management_System.Components;
using Ground_Terminal_Management_System.Data;
using Ground_Terminal_Management_System.Services;
using Microsoft.EntityFrameworkCore;


var tcpReader = new TcpMessageReaderService(5000);
tcpReader.Start();

var builder = WebApplication.CreateBuilder(args);

// Register Razor Components and enable interactive server components.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();



// Register the DbContext for TelemetryContext using the connection string from appsettings.json.
builder.Services.AddDbContext<FdmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FDMSDatabase")));

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

app.Run();
