using Microsoft.Extensions.Configuration.Json;
using TicTacToe.Api.Interfaces;
using TicTacToe.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Render deployment fix --------------------------------------------------
// Containers on Render enforce a low inotify instance limit (128). The host
// adds appsettings.json / appsettings.{Environment}.json with reloadOnChange:
// true, and each watched JSON source opens an inotify watcher, crashing
// startup. Replace those sources IN PLACE (same order, no duplicates) with
// unwatched equivalents. Nothing else about configuration changes.
for (var i = builder.Configuration.Sources.Count - 1; i >= 0; i--) {
    if (builder.Configuration.Sources[i] is JsonConfigurationSource json && json.ReloadOnChange) {
        builder.Configuration.Sources[i] = new JsonConfigurationSource {
            Path = json.Path,
            Optional = json.Optional,
            ReloadOnChange = false
        };
    }
}

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Register services with DI
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IScoreboardService, ScoreboardService>();
builder.Services.AddSingleton<IComputerPlayerService, ComputerPlayerService>();

// Configure CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Startup diagnostics: fail loudly in logs if the Angular bundle is missing from the image.
// If index.html is absent, every SPA route (including "/") returns a bare 404.
var webRootPath = app.Environment.WebRootPath;
var hasIndexHtml = webRootPath is not null && File.Exists(Path.Combine(webRootPath, "index.html"));
app.Logger.LogInformation("SPA static files: web root '{WebRoot}', index.html {Status}",
    webRootPath ?? "MISSING", hasIndexHtml ? "found" : "NOT FOUND");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAngular");

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = "no-store";
    }
});

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
