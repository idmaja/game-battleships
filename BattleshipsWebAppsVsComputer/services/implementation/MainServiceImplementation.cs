using System.Text;
using Serilog;

public class MainService : IMainService
{
    private readonly IGameState _state;
    private readonly Serilog.ILogger _logger;
    private readonly IMessageService _messageService;
    private readonly Random _rand;

    public event Action<string>? OnGameResult;
    
    public MainService(IMessageService messageService, Random rand, IGameState state)
    {
        _messageService = messageService;
        _rand = rand;
        _state = state;

        _logger = Log.ForContext<MainService>();

        OnGameResult += (message) => _logger.Information($"\n{message}\n");
    }

    public Result<object> InitializeGame(CreateGameRequest request)
    {
        _logger.Information("Initializing game...");

        if (request == null)
        {
            _logger.Error("Request cannot be null");
            return Result<object>.Failed("Request cannot be null");
        }

        if (request.PlayerName == null || request.ComputerName == null)
        {
            _logger.Error("Player name and computer name cannot be null");
            return Result<object>.Failed("Player name and computer name cannot be null");
        }

        if (request.ShipLengthsPlayer.Count == 0 || request.ShipLengthsComputer.Count == 0)
        {
            _logger.Error("Ship lengths cannot be empty");
            return Result<object>.Failed("Ship lengths cannot be empty");
        }

        if (request.BoardWidth < 5 || request.BoardHeight < 5)
        {
            _logger.Error("Board width and height must be at least 5");
            return Result<object>.Failed("Board width and height must be at least 5");
        }

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

        _state.Players = new List<IPlayer> { player, computer };

        _state.PlayerBoard = new Dictionary<IPlayer, IBoard>
        {
            { player, boardPlayer },
            { computer, boardComputer }
        };

        _state.PlayerShips = new Dictionary<IPlayer, List<IShip>>
        {
            { player, ships1 },
            { computer, ships2 }
        };
        
        _state.PlayerScores = new Dictionary<IPlayer, int>
        {
            { player, 0 },
            { computer, 0 }
        };

        var board = _state.PlayerBoard[_state.Players[1]];
        foreach (var ship in ships2)
        {
            bool isPlaced = false;

            while (!isPlaced)
            {
                bool horizontal = _rand.Next(2) == 0; // diacak kemungkinan horizontal 50% == 0 dan vertikal 50% == 1

                int row; 
                int col;

                if (horizontal)
                {
                    // row bebas, col dibatasi supaya muat ship.Length
                    row = _rand.Next(0, board.Height);
                    col = _rand.Next(0, board.Width - ship.Length + 1);

                    string start = $"{(char)('A' + col)}{row + 1}";
                    string end   = $"{(char)('A' + col + ship.Length - 1)}{row + 1}";

                    var placed = PlaceShips(computer, ship, start, end);
                    isPlaced = placed.Success;
                }
                else
                {
                    // col bebas, row dibatasi supaya muat ship.Length
                    col = _rand.Next(0, board.Width);
                    row = _rand.Next(0, board.Height - ship.Length + 1);

                    string start = $"{(char)('A' + col)}{row + 1}";
                    string end   = $"{(char)('A' + col)}{row + ship.Length}";

                    var placed = PlaceShips(computer, ship, start, end);
                    isPlaced = placed.Success;
                }
            }
        }

        return Result<object>.Ok("Successfully Initiate the Game");
    }

    public Result<object> ResetGane()
    {
        try
        {
            _state.Players.Clear();
            _state.PlayerBoard.Clear();
            _state.PlayerShips.Clear();
            _state.PlayerScores.Clear();
            
            return Result<object>.Ok("Successfully reset the game");
        }
        catch (Exception ex)
        {
            return Result<object>.Failed($"Failed to reset the game {ex.Message}");
        }
    }

    public Result<object> PlaceShips(IPlayer player, IShip ship, string coorStart, string coorEnd)
    {
        if (IsGameInitialized())
        {
            return Result<object>.Failed($"No Game Found!");
        }

        var board = _state.PlayerBoard[player];

        Coordinate start = CoordinateInput(coorStart);
        Coordinate end = CoordinateInput(coorEnd);

        if (start.Row < 0 || start.Row >= board.Height ||
            end.Row < 0 || end.Row >= board.Height ||
            start.Col < 0 || start.Col >= board.Width ||
            end.Col < 0 || end.Col >= board.Width)
        {
            return Result<object>.Failed($"Invalid ship placement. Ship is outside the board.");
        }
        
        bool isHorizontal = start.Row == end.Row;
        bool isVertical = start.Col == end.Col;

        if (!isHorizontal && !isVertical)
        {
            return Result<object>.Failed($"Invalid ship placement. Ships must be placed horizontally or vertically.");
        }

        int distance = isHorizontal
            ? Math.Abs(end.Col - start.Col) + 1
            : Math.Abs(end.Row - start.Row) + 1;

        if (distance != ship.Length)
        {
            return Result<object>.Failed($"Ship length must be {ship.Length}");
        }

        try
        {
            List<Coordinate> coordinates = PlaceShipInPath(start, end);

            foreach (var coordinate in coordinates)
            {
                if (board.Cells[coordinate.Row, coordinate.Col].Ship != null)
                {
                    return Result<object>.Failed($"Invalid ship placement. Ships cannot overlap or extend beyond the board.");
                }
            }

            foreach (var coordinate in coordinates)
            {
                board.Cells[coordinate.Row, coordinate.Col].Ship = (Ship?)ship;
            }

            ship.Positions = coordinates;
            return Result<object>.Ok($"Ship placed successfully for player {player.Name} at coordinates: {coorStart} to {coorEnd}");
        }
        catch (Exception ex)
        {
            return Result<object>.Failed($"Error placing ship for player {player.Name}: {ex.Message}");
        }
    }

    public void RemoveShip(IPlayer player, IShip ship) {

        if (IsGameInitialized())
        {
            return Result<object>.Failed($"No Game Found!"); 
        }

        var board = _state.PlayerBoard[player];

        if (ship.Positions != null)
        {
            foreach (var pos in ship.Positions)
            {
                bool inside =
                    pos.Row >= 0 && pos.Row < board.Height &&
                    pos.Col >= 0 && pos.Col < board.Width;

                if (inside && board.Cells[pos.Row, pos.Col].Ship == ship)
                    board.Cells[pos.Row, pos.Col].Ship = null;
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
            {
                coordinates.Add(new Coordinate(coorStart.Row, col));
            }
        }
        else if (coorStart.Col == coorEnd.Col)
        {
            int startRow = Math.Min(coorStart.Row, coorEnd.Row);
            int endRow = Math.Max(coorStart.Row, coorEnd.Row);
            
            for (int row = startRow; row <= endRow; row++)
            {
                coordinates.Add(new Coordinate(row, coorStart.Col));
            }
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

            if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
            {
                _logger.Error($"Invalid input. Please enter a valid coordinate (e.g., A1, B4, and so on).");
                return new Coordinate(0,0);
            }

            return new Coordinate(letterRow - 1, letterCol);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error Coordinate input: {ex.Message}");
            return new Coordinate(0, 0);
        }
    }

    public async Task<Result<AttackResult>> Attack(Coordinate coordinate)
    {
        var human = _state.Players[0];
        var computer = _state.Players[1];
        var scores = GetAllPlayerScore();

        var cellComputer = _state.PlayerBoard[computer].Cells[coordinate.Row, coordinate.Col];

        bool isShipHit = ReceivedAttack(computer, coordinate, out string _);

        var messageNotification = new StringBuilder();

        if (isShipHit)
        {
            IncreasePlayerScore(human);
            
            if (IsAllShipsSunk(computer))
            {
                GameResult($"Winner: {human.Name} ({scores[human]}) | Looser: {computer.Name} ({scores[computer]})");

                await MessageNotification(
                    $"Winner: {human.Name} ({scores[human]}) | Looser: {computer.Name} ({scores[computer]})"
                );

                return Result<AttackResult>.Ok(new AttackResult
                {
                    HumanHit = true,
                    ComputerHit = false,
                    IsGameOver = true,
                });
            }

            messageNotification.Append($"Attack by {human.Name} hit the target! | ");
        }
        else
        {
            cellComputer.IsHit = true;
            messageNotification.Append($"Attack by {human.Name} is off target! | ");
        }

        var coordinateComputerShot = GetRandomShotForComputer(human);
        bool computerHit = ReceivedAttack(human, coordinateComputerShot, out string _);
        var cellHuman = _state.PlayerBoard[human].Cells[coordinateComputerShot.Row, coordinateComputerShot.Col];

        if (computerHit)
        {
            IncreasePlayerScore(computer);

            if (IsAllShipsSunk(human))
            {
                GameResult($"Winner: {computer.Name} ({scores[computer]}) | Looser: {human.Name} ({scores[human]})");

                await MessageNotification(
                    $"Winner: {computer.Name} ({scores[computer]}) | Looser: {human.Name} ({scores[human]})"
                );

               return Result<AttackResult>.Ok(new AttackResult
                {
                    HumanHit = isShipHit,
                    ComputerHit = true,
                    IsGameOver = true,
                    Coordinate = coordinateComputerShot
                });
            }

            messageNotification.Append($"Attack by {computer.Name} hit the target!");
        }
        else
        {
            cellHuman.IsHit = true;
            messageNotification.Append($"Attack by {computer.Name} is off target!");
        }

        // send message through SignalR / WebSocket
        await MessageNotification(messageNotification.ToString());

        return Result<AttackResult>.Ok(new AttackResult
        {
            HumanHit = isShipHit,
            ComputerHit = computerHit,
            IsGameOver = false,
            Coordinate = coordinateComputerShot
        });
    }

    public Coordinate GetRandomShotForComputer(IPlayer human)
    {
        var board = _state.PlayerBoard[human];

        while (true)
        {
            int row = _rand.Next(0, board.Height);
            int col = _rand.Next(0, board.Width);

            var cell = board.Cells[row, col];

            if (!cell.IsHit)
            {
                return new Coordinate(row, col);
            }
        }
    }

    public bool ReceivedAttack(IPlayer defender, Coordinate coordinate, out string message)
    {
        var boardDefender = _state.PlayerBoard[defender];
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
        {
            cell.Ship.IsSunk = true;
        } 

        message = "Ship hit successfully.";
        cell.IsHit = true;

        return true;
    }

    public bool IsAllShipsSunk(IPlayer defender)
    {
        var shipsDefender = _state.PlayerShips[defender];

        return shipsDefender.All(ship => ship.IsSunk);
    }

    public void IncreasePlayerScore(IPlayer player) => _state.PlayerScores[player]++;
    public void IncreaseShipHit(ICell cell) => cell.Ship!.Hits++;

    public virtual void GameResult(string message) => OnGameResult?.Invoke(message); // synchronous
    public virtual async Task MessageNotification(string message)
    {
        try {
            await _messageService.SendMessageAsync(message); // asynchronous 
        }
        catch(Exception ex) {
            _logger.Warning("Message failed: " + ex.Message);
        } 
    }

    public bool IsGameInitialized()
    {
        return _state.Players.Count > 0 
            && _state.PlayerBoard.Count > 0 
            && _state.PlayerShips.Count > 0 
            && _state.PlayerScores.Count > 0;
    }
    public int GetPlayerScore(IPlayer player) => _state.PlayerScores[player];
    public Dictionary<IPlayer, int> GetAllPlayerScore() => _state.PlayerScores;
    public IBoard GetBoardInfo(IPlayer player) => _state.PlayerBoard[player];
    public IReadOnlyList<IPlayer> GetPlayersInfo() => _state.Players;
    public IReadOnlyDictionary<IPlayer, IBoard> GetPlayerBoards() => _state.PlayerBoard;
    public IReadOnlyList<IShip> GetPlayerShips(IPlayer player) => _state.PlayerShips[player];
}