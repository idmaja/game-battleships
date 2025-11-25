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

    public event Action<string>? OnGameResult;
    
    public MainService(IMessageService messageService)
    {
        _messageService = messageService;
        _logger = Log.ForContext<MainService>();
        _players = new List<IPlayer>() ;
        _playerBoards = new Dictionary<IPlayer, IBoard>();
        _playerShips = new Dictionary<IPlayer, List<IShip>>();
        _playerScores = new Dictionary<IPlayer, int>();
        OnGameResult += (message) => 
            _logger.Information($"\u001b[1m{message}\u001b[0m\n");
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
                bool horizontal = rand.Next(2) == 0; // diacak kemungkinan horizontal 50% == 0 dan vertikal 50% == 1

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
            List<Coordinate> coordinates = PlaceShipInPath(start, end);

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

    public List<Coordinate> PlaceShipInPath(Coordinate coorStart, Coordinate coorEnd)
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
            _logger.Information($"Ship cannot be placed except Horizontal & Vertical");
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
            _logger.Error($"Invalid input. Please enter a valid coordinate (e.g., A1, B4, and so on).");
            return new Coordinate(0, 0);
        }
    }

    public async Task<AttackResult> Attack(Coordinate coordinate)
    {
        var human = _players[0];
        var computer = _players[1];
        var scores = GetAllPlayerScore();

        var cellComputer = _playerBoards[computer].Cells[coordinate.Row, coordinate.Col];

        bool isShipHit = ReceivedAttack(computer, coordinate, out string message);

        var messageNotification = new StringBuilder();

        if (isShipHit)
        {
            IncreasePlayerScore(human);

            // coordinate = {(char)(coordinate.Col + 'A')}{coordinate.Row + 1}
            messageNotification.Append($"Attack by {human.Name} hit the target! | ");
            
            if (IsAllShipsSunk(computer))
            {
                GameResult($"Winner: {human.Name} ({scores[human]}) | Looser: {computer.Name} ({scores[computer]})");

                await MessageNotification(
                    $"Winner: {human.Name} ({scores[human]}) | Looser: {computer.Name} ({scores[computer]})"
                );

                return new AttackResult
                {
                    HumanHit = true,
                    ComputerHit = false,
                    IsGameOver = true,
                };
            }
        }
        else
        {
            cellComputer.IsHit = true;
            messageNotification.Append($"Attack by {human.Name} is off target! | ");
        }

        var coordinateComputerShot = GetRandomShotForComputer(human);
        bool computerHit = ReceivedAttack(human, coordinateComputerShot, out string messageComputer);
        var cellHuman = _playerBoards[human].Cells[coordinateComputerShot.Row, coordinateComputerShot.Col];

        if (computerHit)
        {
            IncreasePlayerScore(computer);

            // coordinate = {(char)(coordinateComputerShot.Col + 'A')}{coordinateComputerShot.Row + 1}
            messageNotification.Append($"Attack by {computer.Name} hit the target!");

            if (IsAllShipsSunk(human))
            {
                GameResult($"Winner: {computer.Name} ({scores[computer]}) | Looser: {human.Name} ({scores[human]})");

                await MessageNotification(
                    $"Winner: {computer.Name} ({scores[computer]}) | Looser: {human.Name} ({scores[human]})"
                );

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
            messageNotification.Append($"Attack by {computer.Name} is off target!");
        }

        await MessageNotification(messageNotification.ToString());

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
            message = "No ship present at the attacked coordinate.</b>";
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

    public virtual void GameResult(string message) 
        => OnGameResult?.Invoke(message); // synchronous

    public virtual async Task MessageNotification(string message)
    {
        // OnMessageReceived?.Invoke(message); // synchronous
        await _messageService.SendMessageAsync(message); // asynchronous 
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
}