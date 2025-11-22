using System.Text;
using Serilog;

public class MainService : IMainService
{
    private List<IPlayer> _players;
    private Dictionary<IPlayer, int> _playerScores;
    private Dictionary<IPlayer, IBoard> _playerBoards;
    private Dictionary<IPlayer, List<IShip>> _playerShips;
    private int _currentPlayerIndex;

    private readonly Serilog.ILogger _logger;
    private readonly IMessageService _messageService;

    // public event Action<IPlayer, IPlayer, Dictionary<IPlayer, int>>? OnMessageResult;
    public event Action<string>? OnMessageReceived;
    
    public MainService(IMessageService messageService)
    {
        _messageService = messageService;
        _players = new List<IPlayer>() ;
        _playerBoards = new Dictionary<IPlayer, IBoard>();
        _playerShips = new Dictionary<IPlayer, List<IShip>>();
        _playerScores = new Dictionary<IPlayer, int>();

        _currentPlayerIndex = 0;
        _logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File("logs/mainservice.log", rollingInterval: RollingInterval.Day)
                    .CreateLogger();
    }

    public void InitializeGame(CreateGameRequest request)
    {
        var player1 = new Player(request.Player1Name);
        var player2 = new Player(request.Player2Name);

        var boardPlayer1 = new Board(request.BoardWidth, request.BoardHeight);
        var boardPlayer2 = new Board(request.BoardWidth, request.BoardHeight);

        var ships1 = request.ShipLengthsPlayer1
            .Select(length => (IShip)new Ship(length, 0, false, new List<Coordinate>()))
            .ToList();

        var ships2 = request.ShipLengthsPlayer2
            .Select(length => (IShip)new Ship(length, 0, false, new List<Coordinate>()))
            .ToList();

        _players = new List<IPlayer> { player1, player2 };

        _playerBoards = new Dictionary<IPlayer, IBoard>
        {
            { player1, boardPlayer1 },
            { player2, boardPlayer2 }
        };

        _playerShips = new Dictionary<IPlayer, List<IShip>>
        {
            { player1, ships1 },
            { player2, ships2 }
        };
        
        _playerScores = new Dictionary<IPlayer, int>
        {
            { player1, 0 },
            { player2, 0 }
        };
        
        _currentPlayerIndex = 0;
    }

    public bool PlaceShips(IPlayer player, IShip ship, string coorStart, string coorEnd, out string message)
    {
        var board = _playerBoards[player];

        Coordinate start = CoordinateInput(coorStart);
        Coordinate end = CoordinateInput(coorEnd);

        if (start.Row < 0 || start.Row >= board.Height ||
            end.Row < 0 || end.Row >= board.Height ||
            start.Col < 0 || start.Col >= board.Width ||
            end.Col < 0 || end.Col >= board.Width)
        {
            message = $"Invalid ship placement. Ship is outside the board.";
            return false;
        }
        
        bool isHorizontal = start.Row == end.Row;
        bool isVertical = start.Col == end.Col;

        if (!isHorizontal && !isVertical)
        {
            message = $"Invalid ship placement. Ships must be placed horizontally or vertically.";
            return false;
        }

        int distance;

        if (isHorizontal) 
            distance = Math.Abs(end.Col - start.Col) + 1;
        else
            distance = Math.Abs(end.Row - start.Row) + 1;

        if (distance != ship.Length)
        {
            message = $"Ship length must be {ship.Length}";
            return false;
        }

        try
        {
            List<Coordinate> coordinates = CheckShipPath(start, end);

            foreach (var coordinate in coordinates)
            {
                if (board.Cells[coordinate.Row, coordinate.Col].Ship != null)
                {
                    message = $"Invalid ship placement. Ships cannot overlap or extend beyond the board.";
                    return false;
                }
            }

            foreach (var coordinate in coordinates)
                board.Cells[coordinate.Row, coordinate.Col].Ship = (Ship?)ship;

            ship.Positions = coordinates;
            message = $"Ship placed successfully for player {player.Name} at coordinates: {coorStart} to {coorEnd}";
            return true;
        }
        catch (Exception)
        {
            message = $"Error placing ship for player {player.Name}";
            return false;
        }
    }

    public List<Coordinate> CheckShipPath(Coordinate coorStart, Coordinate coorEnd)
    {
        
        List<Coordinate> coordinates = new List<Coordinate>();
        
        if (coorStart.Row == coorEnd.Row)
        {
            int startCol = Math.Min(coorStart.Col, coorEnd.Col);
            int endCol = Math.Max(coorStart.Col, coorEnd.Col);
            
            for (int col = startCol; col <= endCol; col++)
                coordinates.Add(new Coordinate(coorStart.Row, col));
        }
        else if (coorStart.Col == coorEnd.Col)
        {
            int startRow = Math.Min(coorStart.Row, coorEnd.Row);
            int endRow = Math.Max(coorStart.Row, coorEnd.Row);
            
            for (int row = startRow; row <= endRow; row++)
                coordinates.Add(new Coordinate(row, coorStart.Col));
        }
        else
        {
            Console.WriteLine("Ships cannot be placed except Horizontal & Vertical");
            return new List<Coordinate>();
        }

        return coordinates;
    }        

    public Coordinate CoordinateInput(string input)
    {
        try
        {
            char letter = char.ToUpper(input[0]);
            int letterCol = letter - 'A';
            bool IsLetter = int.TryParse(input.Substring(1), out int letterRow);

            return new Coordinate(letterRow - 1, letterCol);
        }
        catch
        {
            Console.WriteLine("Invalid input. Please enter a valid coordinate (e.g., A1, B4, and so on).");
            return new Coordinate(0, 0);
        }
    }

    public async Task<bool> Attack(Coordinate coordinate)
    {
        IPlayer attacker = GetCurrentPlayer();
        IPlayer defender = NextPlayer();

        bool isShipHit = ReceivedAttack(defender, coordinate, out string message);

        var outputMessage = new StringBuilder();
        outputMessage.AppendLine($"Winner: {attacker.Name}");
        outputMessage.AppendLine($"Looser: {defender.Name}");
        outputMessage.AppendLine("Score:");

        foreach (var item in GetAllPlayerScore())
            outputMessage.AppendLine($"- {item.Key.Name}: {item.Value}");

        if (isShipHit)
        {
            IncreasePlayerScore(attacker);

            if (IsAllShipsSunk(defender)) 
                await MessageNotification(outputMessage.ToString());
            
            await MessageNotification(message);
        }
        else
        {
            await MessageNotification($"Attack by {attacker.Name} at {(char)(coordinate.Col + 'A')}{coordinate.Row + 1} missed!");
            NextPlayer();
        }
        
        return isShipHit;
    }

    public bool ReceivedAttack(IPlayer defender, Coordinate coordinate, out string message)
    {
        var boardDefender = _playerBoards[defender];
        var cell = boardDefender.Cells[coordinate.Row, coordinate.Col];

        if (cell.IsHit) 
        {
            message = "Cell has already been attacked.";
            return false;
        }
            
        if (cell.Ship == null) 
        {
            message = "No ship present at the attacked coordinate.";
            return false;
        }

        IncreaseShipHit(cell);
        
        if (cell.Ship.Hits >= cell.Ship.Length) 
            cell.Ship.IsSunk = true;

        message = "Ship hit successfully.";
        cell.IsHit = true;

        return true;
    }

    public bool IsAllShipsSunk(IPlayer defender)
    {
        var shipsDefender = _playerShips[defender];

        return shipsDefender.All(ship => ship.IsSunk);
    }

    public void IncreasePlayerScore(IPlayer player) => _playerScores[player]++;
    public void IncreaseShipHit(ICell cell) => cell.Ship!.Hits++;

    // public virtual void MessageNotification(IPlayer attacker, IPlayer defender, Dictionary<IPlayer, int> playerScore) 
        // => OnMessageResult?.Invoke(attacker, defender, _playerScores);
    public virtual async Task MessageNotification(string message)
    {
        OnMessageReceived?.Invoke(message); // synchronous event invocation
        await _messageService.SendMessageAsync(message); // asynchronous message sending
    }

    public bool IsGameInitialized()
    {
        return _players.Count > 0 
            && _playerBoards.Count > 0 
            && _playerShips.Count > 0 
            && _playerScores.Count > 0;
    }
    public IPlayer GetCurrentPlayer() => _players[_currentPlayerIndex];
    public IPlayer NextPlayer() => _players[(_currentPlayerIndex + 1) % _players.Count];
    public int GetPlayerScore(IPlayer player) => _playerScores[player];
    public Dictionary<IPlayer, int> GetAllPlayerScore() => _playerScores;
    public IBoard GetBoardInfo(IPlayer player) => _playerBoards[player];
    public IReadOnlyList<IPlayer> GetPlayersInfo() => _players;
    public IReadOnlyDictionary<IPlayer, IBoard> GetPlayerBoards() => _playerBoards;
    public IReadOnlyList<IShip> GetPlayerShips(IPlayer player) => _playerShips[player];
    public Serilog.ILogger GetLogger() => _logger;
}