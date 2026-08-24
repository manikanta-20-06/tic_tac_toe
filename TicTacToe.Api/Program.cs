using TicTacToe.Api.Interfaces;
using TicTacToe.Api.Services;

var builder = WebApplication.CreateBuilder(args);

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
