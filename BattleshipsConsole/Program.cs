var player1 = new Player("P 1");
var player2 = new Player("P 2");

var board1 = new Board(8, 8);
var board2 = new Board(8, 8);

var ships1 = new List<IShip>
{
    new Ship(5, 0, false, new List<Coordinate>()),
    new Ship(4, 0, false, new List<Coordinate>()),
    new Ship(3, 0, false, new List<Coordinate>())
};

var ships2 = new List<IShip>
{
    new Ship(5, 0, false, new List<Coordinate>()),
    new Ship(4, 0, false, new List<Coordinate>()),
    new Ship(3, 0, false, new List<Coordinate>())
};

var controller = new GameController(player1, player2, board1, board2, ships1, ships2);
var playersScore = controller.GetAllPlayerScore();
var boardsInfo = controller.GetBoardInfo(player1);

controller.OnMessageResult += (winner, loser, playerscore) =>
{
    Console.WriteLine();
    Console.WriteLine("=== GAME RESULT  ===");
    Console.WriteLine($"Winner: {winner.Name}");
    Console.WriteLine($"Loser : {loser.Name}");
    Console.WriteLine($"Winner Score : {playerscore[winner]}");
    Console.WriteLine($"Loser Score : {playerscore[loser]}");
    Console.WriteLine("Thanks for playing.");
    Console.WriteLine();
};

controller.StartGame();