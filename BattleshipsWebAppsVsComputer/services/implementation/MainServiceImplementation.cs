using System.Text;
using Serilog;

public class MainService : IMainService
{
    private List<IPlayer> _players;
    private Dictionary<IPlayer, int> _playerScores;
    private Dictionary<IPlayer, IBoard> _playerBoards;
    private Dictionary<IPlayer, List<IShip>> _playerShips;

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

        _logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File("logs/mainservice.log", rollingInterval: RollingInterval.Day)
                    .CreateLogger();
    }

    public void InitializeGame(CreateGameRequest request)
    {
        var player = new Player(request.PlayerName);
        var computer = new Player(request.ComputerName);

        var boardPlayer = new Board(request.BoardWidth, request.BoardHeight);
        var boardComputer = new Board(request.BoardWidth, request.BoardHeight);

        var ships1 = request.ShipLengthsPlayer
            .Select(length => (IShip)new Ship(length, 0, false, new List<Coordinate>()))
            .ToList();

        var ships2 = request.ShipLengthsComputer
            .Select(length => (IShip)new Ship(length, 0, false, new List<Coordinate>()))
            .ToList();

        _players = new List<IPlayer> { player, computer };

        _playerBoards = new Dictionary<IPlayer, IBoard>
        {
            { player, boardPlayer },
            { computer, boardComputer }
        };

        _playerShips = new Dictionary<IPlayer, List<IShip>>
        {
            { player, ships1 },
            { computer, ships2 }
        };
        
        _playerScores = new Dictionary<IPlayer, int>
        {
            { player, 0 },
            { computer, 0 }
        };

        var board = _playerBoards[_players[1]];
        var rand = new Random();
        foreach (var ship in ships2)
        {
            bool placed = false;

            while (!placed)
            {
                bool horizontal = rand.Next(2) == 0;

                int row;
                int col;

                if (horizontal)
                {
                    // row bebas, col dibatasi supaya muat ship.Length
                    row = rand.Next(0, board.Height);
                    col = rand.Next(0, board.Width - ship.Length + 1);

                    string start = $"{(char)('A' + col)}{row + 1}";
                    string end   = $"{(char)('A' + col + ship.Length - 1)}{row + 1}";

                    placed = PlaceShips(computer, ship, start, end, out _);
                }
                else
                {
                    // col bebas, row dibatasi supaya muat ship.Length
                    col = rand.Next(0, board.Width);
                    row = rand.Next(0, board.Height - ship.Length + 1);

                    string start = $"{(char)('A' + col)}{row + 1}";
                    string end   = $"{(char)('A' + col)}{row + ship.Length}";

                    placed = PlaceShips(computer, ship, start, end, out _);
                }
            }
        }
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

    public void RemoveShip(IPlayer player, IShip ship) {
        var board = _playerBoards[player];

        if (ship.Positions != null)
        {
            foreach (var pos in ship.Positions)
            {
                if (pos.Row >= 0 && pos.Row < board.Height &&
                    pos.Col >= 0 && pos.Col < board.Width)
                {
                    if (board.Cells[pos.Row, pos.Col].Ship == ship)
                    {
                        board.Cells[pos.Row, pos.Col].Ship = null;
                    }
                }
            }

            ship.Positions = new List<Coordinate>();
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

    public async Task<AttackResult> Attack(Coordinate coordinate)
    {
        var human = _players[0];
        var computer = _players[1];

        var cellComputer = _playerBoards[computer].Cells[coordinate.Row, coordinate.Col];

        bool isShipHit = ReceivedAttack(computer, coordinate, out string message);

        var outputMessage = new StringBuilder();
        outputMessage.AppendLine($"Winner: {human.Name}");
        outputMessage.AppendLine($"Looser: {computer.Name}");
        outputMessage.AppendLine("Score:");
        foreach (var item in GetAllPlayerScore())
            outputMessage.AppendLine($"- {item.Key.Name}: {item.Value}");

        if (isShipHit)
        {
            IncreasePlayerScore(human);
            await MessageNotification(
                $"Attack by {human.Name} at {(char)(coordinate.Col + 'A')}{coordinate.Row + 1} success!");
            
            if (IsAllShipsSunk(computer))
            {
                await MessageNotification(outputMessage.ToString());

                return new AttackResult
                {
                    HumanHit = true,
                    ComputerHit = false,
                    IsGameOver = true,// tidak dipakai lagi
                };
            }
        }
        else
        {
            cellComputer.IsHit = true;
            await MessageNotification(
                $"Attack by {human.Name} at {(char)(coordinate.Col + 'A')}{coordinate.Row + 1} missed.");
        }

        var coordinateComputerShot = GetRandomShotForComputer(human);
        bool computerHit = ReceivedAttack(human, coordinateComputerShot, out string messageComputer);
        var cellHuman = _playerBoards[human].Cells[coordinateComputerShot.Row, coordinateComputerShot.Col];

        if (computerHit)
        {
            IncreasePlayerScore(computer);
            await MessageNotification(
                $"Attack by {computer.Name} at {(char)(coordinateComputerShot.Col + 'A')}{coordinateComputerShot.Row + 1} success!");
            if (IsAllShipsSunk(human))
            {

                await MessageNotification($"{computer.Name} wins");
                return new AttackResult
                {
                    HumanHit = isShipHit,
                    ComputerHit = true,
                    IsGameOver = true,
                    Coordinate = coordinateComputerShot
                };
            }
        }
        else
        {
            cellHuman.IsHit = true;
            await MessageNotification(
                $"Attack by {computer.Name} at {(char)(coordinateComputerShot.Col + 'A')}{coordinateComputerShot.Row + 1} missed!.");
        }

        return new AttackResult
        {
            HumanHit = isShipHit,
            ComputerHit = computerHit,
            IsGameOver = false,
            Coordinate = coordinateComputerShot
        };
    }

    public Coordinate GetRandomShotForComputer(IPlayer human)
    {
        var board = _playerBoards[human];
        var rand = new Random();

        while (true)
        {
            int row = rand.Next(0, board.Height);
            int col = rand.Next(0, board.Width);

            var cell = board.Cells[row, col];

            if (!cell.IsHit)
            {
                return new Coordinate(row, col);
            }
        }
    }
    public Coordinate GetRandomCoordinateForComputer(IPlayer computer)
    {
        var board = _playerBoards[computer];
        var rand = new Random();

        while (true)
        {
            int row = rand.Next(0, board.Height);
            int col = rand.Next(0, board.Width);

            return new Coordinate(row, col);
        }
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
            message = "<b>No ship present at the attacked coordinate.</b>";
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
    public int GetPlayerScore(IPlayer player) => _playerScores[player];
    public Dictionary<IPlayer, int> GetAllPlayerScore() => _playerScores;
    public IBoard GetBoardInfo(IPlayer player) => _playerBoards[player];
    public IReadOnlyList<IPlayer> GetPlayersInfo() => _players;
    public IReadOnlyDictionary<IPlayer, IBoard> GetPlayerBoards() => _playerBoards;
    public IReadOnlyList<IShip> GetPlayerShips(IPlayer player) => _playerShips[player];

    public Serilog.ILogger GetLogger() => _logger;
}