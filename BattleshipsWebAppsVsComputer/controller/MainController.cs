using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/v1/battleships")]
public class MainController : ControllerBase
{
    private readonly IMainService _mainService;
    private readonly Serilog.ILogger _logger;

    public MainController(IMainService mainService)
    {
        _mainService = mainService;
        _logger = Log.ForContext<MainController>();
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
            var players = _mainService.GetPlayersInfo()
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

    [HttpGet("ships")]
    [SwaggerOperation(
        Summary = "Get the ships of a player",
        Description = "Retrieves the ships for a specific player",
        OperationId = "GetPlayerShips"
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse))]
    public IActionResult GetShips([FromQuery] GetShipsRequest request)
    {
        try
        {
            var players = _mainService.GetPlayersInfo().FirstOrDefault(player => player.Name.ToLower() == request.PlayerName.ToLower());
            if (players== null)
            {
                _logger?.Error($"Player {request.PlayerName} not found.");
                return NotFound(new GlobalResponse
                {
                    Success = false,
                    Message = $"Player {request.PlayerName} not found!",
                    Data = {}
                });
            }
            var ships = _mainService.GetPlayerShips(players);
            if (ships == null)
            {
                _logger?.Error($"{request.PlayerName}'s ship(s) are not available or have already been deployed.");
                return NotFound(new GlobalResponse
                {
                    Success = false,
                    Message = $"{request.PlayerName}'s ship(s) are not available or have already been deployed.",
                    Data = {}
                });
            }

            _logger?.Information($"Player {request.PlayerName}'s ships retrieved successfully");
            return Ok(new GlobalResponse
            {
                Success = true,
                Message = $"Player {request.PlayerName}'s ships retrieved successfully.",
                Data = ships
            });
        }
        catch (Exception ex)
        {
            _logger?.Error($"Error retrieving {request.PlayerName}'s ships. {ex.Message}");
            return BadRequest(new GlobalResponse
            {
                Success = false,
                Message = $"Error retrieving {request.PlayerName}'s ships. {ex.Message}",
                Data = { }
            });
        }
    }

    [HttpGet("board")]
    [SwaggerOperation(
        Summary = "Get the board of a player",
        Description = "Retrieves the board for a specific player",
        OperationId = "GetPlayerBoard"
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BoardResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse))]
    public IActionResult GetBoard([FromQuery] GetBoardRequest request)
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
                var players = _mainService.GetPlayersInfo().FirstOrDefault(player => player.Name == request.PlayerName);
                if (players == null)
                {
                    _logger?.Error($"Player {request.PlayerName} not found!");
                    return NotFound($"Player {request.PlayerName} not found!");
                }

                var board = _mainService.GetBoardInfo(players);
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

                _logger?.Information($"{request.PlayerName}'s board retrieved successfully!");
                return Ok(boardResponse);
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error retrieving {request.PlayerName}'s board. {ex.Message}");
                return BadRequest(new GlobalResponse
                {
                    Success = false,
                    Message = $"Error retrieving {request.PlayerName}'s board. Error: {ex.Message}",
                    Data = { }
                });
            }
        }
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
                var players = _mainService
                            .GetPlayersInfo()
                            .FirstOrDefault(player => player.Name == request.PlayerName);
                if (players == null)
                {
                    _logger?.Error($"Player {request.PlayerName} not found!");
                    return NotFound(new GlobalResponse
                    {
                        Success = false,
                        Message = $"Player {request.PlayerName} not found!",
                        Data = {}
                    });
                }

                var ships = _mainService
                            .GetPlayerShips(players)
                            .FirstOrDefault(ship => ship.Length == request.ShipLength);
                if (ships == null)
                {
                    _logger?.Error($"Ship with length {request.ShipLength} is not available or has already been placed");
                    return NotFound(new GlobalResponse
                    {
                        Success = false,
                        Message = $"Ship with length {request.ShipLength} is not available or has already been placed",
                        Data = {}
                    });
                }

                bool success = _mainService.PlaceShips(players, ships, request.Start, request.End, out string message);

                if (!success)
                {
                    _logger?.Error(message);
                    return BadRequest(new GlobalResponse
                    {
                        Success = false,
                        Message = "Ship placement failed. Check the coordinates and length of the ship!",
                        Data = {}
                    });
                }

                _logger?.Information(message);
                return Ok(new GlobalResponse
                {
                    Success = true,
                    Message = "Ship successfully placed",
                    Data = ships
                });
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error during placing ship. {ex.Message}");
                return BadRequest(new GlobalResponse
                {
                    Success = false,
                    Message = $"Error during placing ship. {ex.Message}",
                    Data = {}
                });
            }
        }
    }

    [HttpPost("remove-ship")]
    [SwaggerOperation(
        Summary = "Remove a ship on the board",
        Description = "Removes a ship for a specific player on the board",
        OperationId = "RemovePlayerShip"
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse))]
    public IActionResult RemoveShip([FromBody] RemoveShipRequest request)
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
                var players = _mainService
                            .GetPlayersInfo()
                            .FirstOrDefault(player => player.Name == request.PlayerName);
                if (players == null)
                {
                    _logger?.Error($"Player {request.PlayerName} not found!");
                    return NotFound(new GlobalResponse
                    {
                        Success = false,
                        Message = $"Player {request.PlayerName} not found!",
                        Data = {}
                    });
                }

                var ships = _mainService
                            .GetPlayerShips(players)
                            .FirstOrDefault(ship => ship.Length == request.ShipLength && ship.Positions != null);
                if (ships == null)
                {
                    _logger?.Error($"Ship with length {request.ShipLength} is not available");
                    return NotFound(new GlobalResponse
                    {
                        Success = false,
                        Message = $"Ship with length {request.ShipLength} is not available",
                        Data = {}
                    });
                }

                _mainService.RemoveShip(players, ships);

                _logger?.Information($"Player {players.Name}'s Ship length {request.ShipLength} removed.");
                return Ok(new GlobalResponse
                {
                    Success = true,
                    Message = "Ship successfully removed",
                    Data = {}
                });
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error during removing ship. {ex.Message}");
                return BadRequest(new GlobalResponse
                {
                    Success = false,
                    Message = $"Error during removing ship. {ex.Message}",
                    Data = {}
                });
            }
        }
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
                var coordinate = _mainService.CoordinateInput(request.Coordinate);
                var result = await _mainService.Attack(coordinate);
                var scores = _mainService.GetAllPlayerScore()
                            .ToDictionary(x => x.Key.Name, x => x.Value);

                return Ok(new AttackResponse
                {
                    IsHit = result.HumanHit,
                    ComputerHit = result.ComputerHit,
                    ComputerShot = $"{(char)(result.Coordinate.Col + 'A')}{result.Coordinate.Row + 1}",
                    IsGameOver = result.IsGameOver,
                    Message = result.HumanHit ? "You hit the enemy ship" : "You missed",
                    Scores = scores
                });
                
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error during attack. {ex.Message}");
                return BadRequest(new GlobalResponse
                {
                    Success = false,
                    Message = $"Error during attack. Error : {ex.Message}",
                    Data = {}
                });
            }
        }
    }
}