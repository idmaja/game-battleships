using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Console(
        theme: AnsiConsoleTheme.Code,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.Logger(fileLogger => fileLogger
        .Filter.ByExcluding(logEvent =>
        {
            var message = logEvent.RenderMessage();
            var level = logEvent.Level;

            if (level == LogEventLevel.Debug) return true;
            
            // --- (WHITELIST) ---
            if (message.Contains("Hosting started")) return false;
            if (message.Contains("Now listening on")) return false;

            // --- (BLACKLIST) ---
            if (message.Contains("Hosting") || message.Contains("Load") ||
                message.Contains("Application") || message.Contains("Content") ||
                message.Contains("Request") || message.Contains("Response") ||
                message.Contains("candidate") || message.Contains("Execut") ||
                message.Contains("Route") || message.Contains("Attempting") ||
                message.Contains("Connection") || message.Contains("Done") ||
                message.Contains("No information") || message.Contains("The request") ||
                message.Contains("Establishing") || message.Contains("CORS") ||
                message.Contains("Wildcard") || message.Contains("formatter") ||
                message.Contains("Found protocol") || message.Contains("Completed") ||
                message.Contains("OnConnectedAsync") || message.Contains("Sending") ||
                message.Contains("board") || message.Contains("Endpoint") || message.Contains("Registered"))
            {
                return true;
            }

            // Error, Warning, or self-log included
            return false; 
        })
        .WriteTo.File("logs/mainservice-.log", rollingInterval: RollingInterval.Day)
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen(config =>
{
    config.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Battleships Game (VS Computer) API",
        Version = "v1",
        Description = "API for Battleships Game (VS Computer)"
    });
    config.EnableAnnotations();
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500", "http://localhost:3000", "http://172.168.100.25:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddSingleton<IMainService, MainService>();
builder.Services.AddSingleton<IMessageService, MessageService>();
// REVISI DEPEDENCY INJECTION
builder.Services.AddSingleton<Random>();
builder.Services.AddSingleton<IGameState, GameState>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var mainService = scope.ServiceProvider.GetRequiredService<IMainService>();
    
    mainService.OnGameResult += message =>
        Log.ForContext<MainService>().Information("\n{Message}\n", message);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(config => // file server
    {
        config.RouteTemplate = "battleships/api-docs/{documentName}/battleship-api.json";
    });

    app.UseSwaggerUI(config => // UI
    {
        config.SwaggerEndpoint("/battleships/api-docs/v1/battleship-api.json", "Battleships Game API v1");
        config.RoutePrefix = "battleships/api-docs";
    });
}

app.UseCors();
app.UseHttpsRedirection();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Game}/{action=Index}/{id?}");

app.MapHub<MessageHub>("/gameHub");

app.Run();