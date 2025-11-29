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
        if (IsGameInitialized())
        {
            _logger.Error($"[INITIALIZE GAME] Game already initialized");
            return Result<object>.Failed($"Game already initialized. Please reset the game first.");
        }

        if (request == null)
        {
            _logger.Error($"[INITIALIZE GAME] Request cannot be null");
            return Result<object>.Failed($"Request cannot be null");
        }

        if (request.PlayerName == null || request.ComputerName == null)
        {
            _logger.Error($"[INITIALIZE GAME] Player Name and Computer Name cannot be null");
            return Result<object>.Failed($"Player Name and Computer Name  cannot be null");
        }

        if (request.ShipLengthsPlayer.Count == 0 || request.ShipLengthsComputer.Count == 0)
        {
            _logger.Error($"[INITIALIZE GAME] Ship Lengths cannot be empty");
            return Result<object>.Failed($"Ship Lengths cannot be empty");
        }

        if (request.BoardWidth < 5 || request.BoardHeight < 5)
        {
            _logger.Error($"[INITIALIZE GAME] Board Width and Height must be at least 5");
            return Result<object>.Failed($"Board Width and Height must be at least 5");
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
                // random possibility : 50%
                bool horizontal = _rand.Next(2) == 0; 

                int row; 
                int col;

                if (horizontal)
                {
                    // row is free, col is limited to fit ship.Length
                    row = _rand.Next(0, board.Height);
                    col = _rand.Next(0, board.Width - ship.Length + 1);

                    string start = $"{(char)('A' + col)}{row + 1}";
                    string end   = $"{(char)('A' + col + ship.Length - 1)}{row + 1}";

                    var placed = PlaceShips(computer, ship, start, end);
                    if (!placed.Success)
                    {
                        _logger.Error($"[INITIALIZE GAME] {placed.Error}");
                        return Result<object>.Failed(placed.Error);
                    }
                    else
                    {
                        _logger.Information($"[INITIALIZE GAME] {placed.Value}");
                        isPlaced = placed.Success;
                    }
                }
                else
                {
                    // col is free, row is limited to fit ship.Length
                    col = _rand.Next(0, board.Width);
                    row = _rand.Next(0, board.Height - ship.Length + 1);

                    string start = $"{(char)('A' + col)}{row + 1}";
                    string end   = $"{(char)('A' + col)}{row + ship.Length}";

                    var placed = PlaceShips(computer, ship, start, end);
                    if (!placed.Success)
                    {
                        _logger.Error($"[INITIALIZE GAME] {placed.Error}");
                        return Result<object>.Failed(placed.Error);
                    }
                    else
                    {
                        _logger.Information($"[INITIALIZE GAME] {placed.Value}");
                        isPlaced = placed.Success;
                    }
                }
            }
        }

        return Result<object>.Ok("Game initialized successfully.");
    }

    public Result<object> ResetGane()
    {
        if (!IsGameInitialized())
        {
            _logger.Error("[RESET GAME] Failed to reset the game, no game found!");
            return Result<object>.Failed($"No Game Found!");
        }
        
        try
        {
            _state.Players.Clear();
            _state.PlayerBoard.Clear();
            _state.PlayerShips.Clear();
            _state.PlayerScores.Clear();
            
            _logger.Information("[RESET GAME] Successfully reset the game");
            return Result<object>.Ok("Successfully reset the game");
        }
        catch (Exception ex)
        {
            _logger.Error($"[RESET GAME] Failed to reset the game: {ex.Message}");
            return Result<object>.Failed($"Failed to reset the game: {ex.Message}");
        }
    }

    public Result<object> PlaceShips(IPlayer player, IShip ship, string coorStart, string coorEnd)
    {
        if (!IsGameInitialized())
        {
            _logger.Error("[PLACE SHIP] Failed to place ship, no game found!");
            return Result<object>.Failed($"No Game Found!");
        }

        var board = _state.PlayerBoard[player];

        var coorInputstart = CoordinateInput(coorStart);
        var coorInputend = CoordinateInput(coorEnd);
        if (!coorInputstart.Success || !coorInputend.Success)
        {
            _logger.Error($"[PLACE SHIP] {coorInputstart.Error}");
            return Result<object>.Failed(coorInputstart.Error);
        }

        Coordinate start = coorInputstart.Value;
        Coordinate end = coorInputend.Value;

        if (start.Row < 0 || start.Row >= board.Height ||
            end.Row < 0 || end.Row >= board.Height ||
            start.Col < 0 || start.Col >= board.Width ||
            end.Col < 0 || end.Col >= board.Width)
        {
            _logger.Error("[PLACE SHIP] Invalid ship placement. Ship is outside the board.");
            return Result<object>.Failed($"Invalid ship placement. Ship is outside the board.");
        }
        
        bool isHorizontal = start.Row == end.Row;
        bool isVertical = start.Col == end.Col;

        if (!isHorizontal && !isVertical)
        {
            _logger.Error("[PLACE SHIP] Invalid ship placement. Ships must be placed horizontally or vertically.");
            return Result<object>.Failed($"Invalid ship placement. Ships must be placed horizontally or vertically.");
        }

        int distance;

        if (isHorizontal) 
            distance = Math.Abs(end.Col - start.Col) + 1;
        else
            distance = Math.Abs(end.Row - start.Row) + 1;

        if (distance != ship.Length)
        {
            _logger.Error($"[PLACE SHIP] Ship length must be {ship.Length}");
            return Result<object>.Failed($"Ship length must be {ship.Length}");
        }

        try
        {
            var placeShip = PlaceShipInPath(start, end);
            if (!placeShip.Success)
            {
                _logger.Error($"[PLACE SHIP] Error placing ship for player {player.Name}: {placeShip.Error}");
                return Result<object>.Failed($"Error placing ship for player {player.Name}: {placeShip.Error}");
            }

            var coordinates = placeShip.Value;

            foreach (var coordinate in coordinates)
            {
                if (board.Cells[coordinate.Row, coordinate.Col].Ship != null)
                {
                    _logger.Error($"[PLACE SHIP] Invalid ship placement. Ships cannot overlap or extend beyond the board.");
                    return Result<object>.Failed($"Invalid ship placement. Ships cannot overlap or extend beyond the board.");
                }
            }

            foreach (var coordinate in coordinates)
            {
                board.Cells[coordinate.Row, coordinate.Col].Ship = (Ship?)ship;
            }

            ship.Positions = coordinates;
            _logger.Information($"[PLACE SHIP] Ship placed successfully for player {player.Name} at coordinates: {coorStart} to {coorEnd}");
            return Result<object>.Ok($"Ship placed successfully for player {player.Name} at coordinates: {coorStart} to {coorEnd}");
        }
        catch (Exception ex)
        {
            _logger.Error($"[PLACE SHIP] Error placing ship for player {player.Name}: {ex.Message}");
            return Result<object>.Failed($"Error placing ship for player {player.Name}: {ex.Message}");
        }
    }

    public Result<object> RemoveShip(IPlayer player, IShip ship) {

        if (!IsGameInitialized())
        {
            _logger.Error("[REMOVE SHIP] Failed to remove ship, no game found!");
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
            _logger.Information("[REMOVE SHIP] Ship removed successfully!");
            return Result<object>.Ok($"Ship removed successfully!"); 
        }
        else
        {
            _logger.Warning("[REMOVE SHIP] There are no ship in this Coordinate!");
            return Result<object>.Failed($"There are no ship in this Coordinate!"); 
        }

    }

    public Result<List<Coordinate>> PlaceShipInPath(Coordinate coorStart, Coordinate coorEnd)
    {
        
        List<Coordinate> coordinates = new List<Coordinate>();

        try
        {
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

            _logger.Information($"[PLACE SHIP PATH] Ship placed successfully!");
            return Result<List<Coordinate>>.Ok(coordinates);
        }
        catch (Exception ex)
        {
            _logger.Error($"[PLACE SHIP PATH] Error - Ship cannot be placed except Horizontal & Vertical: {ex.Message}");
            return Result<List<Coordinate>>.Failed($"Error - Ship cannot be placed except Horizontal & Vertical: {ex.Message}");
        }
    }        

    public Result<Coordinate> CoordinateInput(string input)
    {
        try
        {
            char letter = char.ToUpper(input[0]);
            int letterCol = letter - 'A';
            bool IsLetter = int.TryParse(input.Substring(1), out int letterRow);

            if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
            {
                _logger.Error($"[COORDINATE INPUT] Invalid input. Please enter a valid coordinate (e.g., A1, B4, and so on).");
                return Result<Coordinate>.Ok(new Coordinate(0,0));
            }

            return Result<Coordinate>.Ok(new Coordinate(letterRow - 1, letterCol));
        }
        catch (Exception ex)
        {
            _logger.Error($"[COORDINATE INPUT] Error Coordinate input: {ex.Message}");
            return Result<Coordinate>.Failed(ex.Message);
        }
    }

    public async Task<Result<AttackResult>> Attack(Coordinate coordinate)
    {
        if (!IsGameInitialized())
        {
            return Result<AttackResult>.Failed($"No Game Found!"); 
        }

        var human = _state.Players[0];
        var computer = _state.Players[1];
        var scores = GetPlayerScore();
        if (!scores.Success)
        {
            _logger.Error($"[ATTACK] {scores.Error}");
            return Result<AttackResult>.Failed(scores.Error);
        }

        var humanScore = scores.Value
                        .Where(dict => dict.Key.ToString() == human.Name)
                        .Select(dict => dict.Value)
                        .FirstOrDefault();

        var computerScore = scores.Value
                        .Where(dict => dict.Key.ToString() == computer.Name)
                        .Select(dict => dict.Value)
                        .FirstOrDefault();

        var cellComputer = _state.PlayerBoard[computer].Cells[coordinate.Row, coordinate.Col];

        bool isShipHit = ReceivedAttack(computer, coordinate, out string _);

        var messageNotification = new StringBuilder();

        if (isShipHit)
        {
            IncreasePlayerScore(human);
            
            if (IsAllShipsSunk(computer))
            {
                GameResult($"Winner: {human.Name} ({humanScore}) | Looser: {computer.Name} ({computerScore})");

                await MessageNotification(
                    $"Winner: {human.Name} ({humanScore}) | Looser: {computer.Name} ({computerScore})"
                );

                _logger.Information($"[Attack] All Computer's ships is sunk!");
                return Result<AttackResult>.Ok(new AttackResult
                {
                    HumanHit = true,
                    ComputerHit = false,
                    IsGameOver = true,
                });
            }

            _logger.Information($"[Attack] Attack by {human.Name} hit the target!");
            messageNotification.Append($"Attack by {human.Name} hit the target! | ");
        }
        else
        {
            cellComputer.IsHit = true;
            _logger.Information($"[Attack] Attack by {human.Name} is off target!");
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
                GameResult($"Winner: {computer.Name} ({computerScore}) | Looser: {human.Name} ({humanScore})");

                await MessageNotification(
                    $"Winner: {computer.Name} ({computerScore}) | Looser: {human.Name} ({humanScore})"
                );

                _logger.Information($"[Attack] All Player's ships is sunk!");
                return Result<AttackResult>.Ok(new AttackResult
                {
                    HumanHit = isShipHit,
                    ComputerHit = true,
                    IsGameOver = true,
                    Coordinate = coordinateComputerShot
                });
            }

            _logger.Information($"[Attack] Attack by {computer.Name} hit the target!");
            messageNotification.Append($"Attack by {computer.Name} hit the target!");
        }
        else
        {
            cellHuman.IsHit = true;
            _logger.Information($"[Attack] Attack by {computer.Name} is off target!");
            messageNotification.Append($"Attack by {computer.Name} is off target!");
        }

        // send message through SignalR / WebSocket
        await MessageNotification(messageNotification.ToString());

        _logger.Information(messageNotification.ToString());
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
            _logger.Warning($"[MESSAGE NOTIFICATION] Message failed: {ex.Message}");
        } 
    }

    public bool IsGameInitialized()
    {
        return _state.Players.Count > 0 
            && _state.PlayerBoard.Count > 0 
            && _state.PlayerShips.Count > 0 
            && _state.PlayerScores.Count > 0;
    }
    public Result<IReadOnlyList<IPlayer>> GetPlayersInfo()
    {
        try
        {
            return Result<IReadOnlyList<IPlayer>>.Ok(_state.Players);
        }
        catch (Exception ex)
        {
            _logger.Warning($"[GET PLAYER INFO] Error - {ex.Message}");
            return Result<IReadOnlyList<IPlayer>>.Failed($"Error - {ex.Message}");
        }
    } 
        
    public Result<Dictionary<string, int>> GetPlayerScore()
    {
        var resultDict = new Dictionary<string, int>();

        try
        {
            foreach (var player in GetPlayersInfo().Value)
            {
                var score = _state.PlayerScores[player];
                resultDict[player.Name] = score;
            }

            return Result<Dictionary<string, int>>.Ok(resultDict);
        }
        catch (Exception ex)
        {
            _logger.Warning($"[GET PLAYER SCORE] Error: {ex.Message}");
            return Result<Dictionary<string, int>>.Failed($"Error: {ex.Message}");
        }
    }

    public Result<IReadOnlyList<IShip>> GetPlayerShips(IPlayer player) {
        try
        {
            return Result<IReadOnlyList<IShip>>.Ok(_state.PlayerShips[player]);
        }
        catch (Exception ex)
        {
            _logger.Warning($"[GET PLAYER SHIPS] Error - {ex.Message}");
            return Result<IReadOnlyList<IShip>>.Failed($"Error - {ex.Message}");
        }
    }
    public Result<BoardResponse> GetBoardInfo(IPlayer player)
    {
        try
        {
            var board = _state.PlayerBoard[player];
            var boardResponse = new BoardResponse
            {
                Width = board.Width,
                Height = board.Height
            };

            for (int row = 0; row < board.Height; row++)
            {
                for (int col = 0; col < board.Width; col++)
                {
                    var cell = board.Cells[row, col];
                    boardResponse.Cells.Add(new CellResponse
                        {
                            Row = row,
                            Col = col,
                            HasShip = cell.Ship != null,
                            IsSunk = cell.Ship != null && cell.Ship.IsSunk,
                            IsHit = cell.IsHit
                        });
                }
            }

            return Result<BoardResponse>.Ok(boardResponse);
        }
        catch (Exception ex)
        {
             _logger.Warning($"[GET BOARD INFO] Error - {ex.Message}");
            return Result<BoardResponse>.Failed($"Error - {ex.Message}");
        }
    }
}