var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});
builder.Services.AddSingleton<IMainService>(sp =>
{
    IPlayer player1 = new Player("Player 1");
    IPlayer player2 = new Player("Player 2");
    
    IBoard boardPlayer1 = new Board(10, 10);
    IBoard boardPlayer2 = new Board(10, 10);

    List<IShip> ships1 = new List<IShip>
    {
        new Ship(5, 0, false, new List<Coordinate>()),
        new Ship(4, 0, false, new List<Coordinate>()),
        new Ship(3, 0, false, new List<Coordinate>())
    };

    List<IShip> ships2 = new List<IShip>
    {
        new Ship(5, 0, false, new List<Coordinate>()),
        new Ship(4, 0, false, new List<Coordinate>()),
        new Ship(3, 0, false, new List<Coordinate>())
    };

    return new MainService(player1, player2, boardPlayer1, boardPlayer2, ships1, ships2);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();