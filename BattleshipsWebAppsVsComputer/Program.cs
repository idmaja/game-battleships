using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
            .MinimumLevel.Override("System", LogEventLevel.Fatal)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Fatal)
            .MinimumLevel.Override("Microsoft.AspNetCore.SignalR", LogEventLevel.Fatal)
            .WriteTo.Console(
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.File("logs/mainservice-.log", rollingInterval: RollingInterval.Day)
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
        Description = "API for Battleships Game (VS Computer) Application"
    });
    config.EnableAnnotations();
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500", "http://localhost:3000", "")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddSingleton<IMainService, MainService>();
builder.Services.AddSingleton<IMessageService, MessageService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api-docs/{documentName}/battleship-api.json";
    });

    app.UseSwaggerUI(config =>
    {
        config.SwaggerEndpoint("/api-docs/v1/battleship-api.json", "Battleships Game API v1");
        config.RoutePrefix = "api-docs";
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