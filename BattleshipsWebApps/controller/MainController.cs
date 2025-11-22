using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/v1/battleships")]
public class MainController : ControllerBase
{
    private readonly IMainService _mainService;
    private readonly Serilog.ILogger? _logger;
    private readonly IMessageService _messageService;
    private string? _message;

    public MainController(IMainService mainService, IMessageService messageService)
    {
        _mainService = mainService;
        _logger = _mainService.GetLogger();
        _messageService = messageService;
    }

    [HttpPost("initialize-game")]
    [SwaggerOperation(
        Summary = "Initialize a new Battleships game",
        Description = "Creates a new game state and prepares ships for the player",
        OperationId = "InitializeGame"
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse))]
    public IActionResult InitializeGame([FromBody] CreateGameRequest request)
    {
        try
        {
            _mainService.InitializeGame(request);

            _logger?.Information("Game initialized successfully.");
            return Ok(new GlobalResponse
            {
                Success = true,
                Message = "Game initialized successfully.",
                Data = {}
            });  
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error initializing game.");
            return BadRequest(new GlobalResponse
            {
                Success = false,
                Message = $"Error initializing game. {ex.Message}",
                Data = {}
            });
        }
    }

    [HttpGet("players")]
    [SwaggerOperation(
        Summary = "Get a list of players in the game",
        Description = "Retrieves the current players in the game",
        OperationId = "GetPlayers"
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse))]
    public IActionResult GetPlayers()
    {
        if (!_mainService.IsGameInitialized())
        {
            return BadRequest(new GlobalResponse
            {
                Success = false,
                Message = "No Game found.",
                Data = {}
            });
        }
        else
        {
            var players = _mainService
                .GetPlayersInfo()
                .Select(player => new { Name = player.Name }) // select Name aja
                .ToList();

            if (players.Count == 0)
            {
                _logger?.Error("No players found.");
                return NotFound(new GlobalResponse
                {
                    Success = false,
                    Message = "No players found.",
                    Data = {}
                });
            }

            _logger?.Information("Players retrieved successfully.");
            return Ok(new GlobalResponse
            {
                Success = true,
                Message = "Players retrieved successfully.",
                Data = players
            });
        }
    }

    [HttpGet("scores")]
    [SwaggerOperation(
        Summary = "Get the scores of all players",
        Description = "Retrieves the current scores of all players in the game",
        OperationId = "GetScores"
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse))]
    public IActionResult GetScores()
    {
        if (!_mainService.IsGameInitialized())
        {
            return BadRequest(new GlobalResponse
            {
                Success = false,
                Message = "No Game found.",
                Data = {}
            });
        }
        else
        {
            var resultList = new List<object>();
            var players = _mainService.GetPlayersInfo();

            if (players.Count == 0)
            {
                _logger?.Error("No players found.");
                return BadRequest(new GlobalResponse
                {
                    Success = false,
                    Message = "No players found.",
                    Data = {}
                });
            }

            foreach (var player in players)
            {
                var score = _mainService.GetPlayerScore(player);
                resultList.Add(new Dictionary<string, object>
                {
                    { "Player", player.Name },
                    { "Score", score }
                });
            }

            _logger?.Information("Scores retrieved successfully.");
            return Ok(new GlobalResponse
            {
                Success = true,
                Message = "Scores retrieved successfully.",
                Data = resultList
            });
        }
    }

    [HttpGet("ships/{playerName}")]
    [SwaggerOperation(
        Summary = "Get the ships of a player",
        Description = "Retrieves the ships for a specific player",
        OperationId = "GetPlayerShips"
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse))]
    public IActionResult GetShips(string playerName)
    {
        try
        {
            var player = _mainService.GetPlayersInfo().FirstOrDefault(p => p.Name == playerName);
            if (player == null)
            {
                _logger?.Error($"Player {playerName} not found.");
                return NotFound(new GlobalResponse
                {
                    Success = false,
                        Message = $"Player {playerName} not found!",
                        Data = {}
                });
            }
            var ships = _mainService.GetPlayerShips(player);
            if (ships == null)
            {
                _logger?.Error($"{playerName}'s ship(s) are not available or have already been deployed.");
                return NotFound(new GlobalResponse
                {
                    Success = false,
                    Message = $"{playerName}'s ship(s) are not available or have already been deployed.",
                    Data = {}
                });
            }
            
            _logger?.Information($"{playerName}'s ships retrieved successfully");
            return Ok(new GlobalResponse
            {
                Success = true,
                Message = $"{playerName}'s ships retrieved successfully.",
                Data = ships
            });
        }
        catch (Exception ex)
        {
            _logger?.Error($"Error retrieving {playerName}'s ships. {ex.Message}");
            return BadRequest(new GlobalResponse
            {
                Success = false,
                Message = $"Error retrieving {playerName}'s ships. {ex.Message}",
                Data = { }
            });
        }
    }

    [HttpGet("board/{playerName}")]
    [SwaggerOperation(
        Summary = "Get the board of a player",
        Description = "Retrieves the board for a specific player",
        OperationId = "GetPlayerBoard"
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BoardResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BoardResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BoardResponse))]
    public IActionResult GetBoard(string playerName)
    {
        var player = _mainService.GetPlayersInfo().FirstOrDefault(p => p.Name == playerName);
        if (player == null)
        {
            _logger?.Error($"Player {playerName} not found!");
            return NotFound($"Player {playerName} not found!");
        }

        var board = _mainService.GetBoardInfo(player);
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
                        IsSunk = cell.Ship != null && cell.Ship.IsSunk
                    });
            }
        }

        _logger?.Information($"{playerName}'s board retrieved successfully!");
        return Ok(boardResponse);
    }

    [HttpPost("place-ship")]
    [SwaggerOperation(
        Summary = "Place a ship on the board",
        Description = "Places a ship for a specific player on the board",
        OperationId = "PlacePlayerShip"
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse))]
    public IActionResult PlaceShip([FromBody] PlaceShipRequest request)
    {
        var players = _mainService.GetPlayersInfo();
        var player = players.FirstOrDefault(p => p.Name == request.PlayerName);
        if (player == null)
        {
            _logger?.Error($"Player {request.PlayerName} not found!");
            return NotFound(new GlobalResponse
            {
                Success = false,
                Message = $"Player {request.PlayerName} not found!",
                Data = {}
            });
        }

        var ships = _mainService.GetPlayerShips(player);
        var ship = ships.FirstOrDefault(s => s.Length == request.ShipLength);
        if (ship == null)
        {
            _logger?.Error($"Ship with length {request.ShipLength} is not available or has already been placed");
            return NotFound(new GlobalResponse
            {
                Success = false,
                Message = $"Ship with length {request.ShipLength} is not available or has already been placed",
                Data = {}
            });
        }

        bool success = _mainService.PlaceShips(player, ship, request.Start, request.End, out _message);

        if (!success)
        {
            _logger?.Error(_message);
            return BadRequest(new GlobalResponse
            {
                Success = false,
                Message = "Ship placement failed. Check the coordinates and length of the ship!",
                Data = {}
            });
        }

        _logger?.Information(_message);
        return Ok(new GlobalResponse
        {
            Success = true,
            Message = "Ship successfully placed",
            Data = ships
        });
    }

    [HttpPost("attack")]
    [SwaggerOperation(
        Summary = "Attack a coordinate on the opponent's board",
        Description = "Attacks a specific coordinate on the opponent's board",
        OperationId = "AttackOpponentShip"
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AttackResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(AttackResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(AttackResponse))]
    public async Task<IActionResult> Attack([FromBody] AttackRequest request)
    {
        if (!_mainService.IsGameInitialized())
        {
            return BadRequest(new GlobalResponse
            {
                Success = false,
                Message = "No Game found.",
                Data = {}
            });
        }
        else
        {
            try
            {
                Coordinate coordinate = _mainService.CoordinateInput(request.Coordinate);
                var players = _mainService.GetPlayersInfo();
                var isHit = await _mainService.Attack(coordinate);

                var scores = new Dictionary<string, int>();
                foreach (var player in players)
                    scores[player.Name] = _mainService.GetPlayerScore(player);

                bool isGameOverForDefender = players.Any(defender => _mainService.IsAllShipsSunk(defender));

                _logger?.Information($"Attack retrieved by successfully");
                return Ok(new AttackResponse
                {
                    IsHit = isHit,
                    IsGameOver = isGameOverForDefender,
                    Message = isHit ? "Hit" : "Miss",
                    Scores = scores
                });
                
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error during attack. {ex.Message}");
                return BadRequest(new GlobalResponse
                {
                    Success = false,
                    Message = $"Error during attack. {ex.Message}",
                    Data = {}
                });
            }
        }
    }
}