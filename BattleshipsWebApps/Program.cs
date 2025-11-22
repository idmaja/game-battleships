using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen(config =>
{
    config.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Battleships Game API",
        Version = "v1",
        Description = "API for Battleships Game Application"
    });
    config.EnableAnnotations();
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500", "")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddSingleton<IMainService, MainService>();
builder.Services.AddSingleton<IMessageService, MessageService>();

var app = builder.Build();

var mainService = app.Services.GetRequiredService<IMainService>();
mainService.OnMessageReceived += (message) =>
{
    Console.WriteLine($"\n[INTERNAL METHOD LOG] {message}");
};

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