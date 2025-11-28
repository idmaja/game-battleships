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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse<object>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse<object>))]
    public IActionResult InitializeGame([FromBody] CreateGameRequest request)
    {
        try
        {
            _mainService.InitializeGame(request);

            _logger?.Information("Game initialized successfully.");
            return Ok(new GlobalResponse<object>
            {
                Success = true,
                Message = "Game initialized successfully.",
                Data = null
            });  
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error initializing game.");
            return BadRequest(new GlobalResponse<object>
            {
                Success = false,
                Message = $"Error initializing game. {ex.Message}",
                Data = null
            });
        }
    }

    [HttpGet("players")]
    [SwaggerOperation(
        Summary = "Get a list of players in the game",
        Description = "Retrieves the current players in the game",
        OperationId = "GetPlayers"
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse<List<PlayerResponse>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse<object>))]
    public IActionResult GetPlayers()
    {
        if (!_mainService.IsGameInitialized())
        {
            return NotFound(new GlobalResponse<object>
            {
                Success = false,
                Message = "No Game found.",
                Data = null
            });
        }
        else
        {
            var players = _mainService.GetPlayersInfo()
                        .Select(player => new PlayerResponse { Name = player.Name }) // select Name aja
                        .ToList();

            if (players.Count == 0)
            {
                _logger?.Error("No players found.");
                return NotFound(new GlobalResponse<object>
                {
                    Success = false,
                    Message = "No players found.",
                    Data = null
                });
            }

            _logger?.Information("Players retrieved successfully.");
            return Ok(new GlobalResponse<List<PlayerResponse>>
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse<List<ScoresResponse>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse<object>))]
    public IActionResult GetScores()
    {
        if (!_mainService.IsGameInitialized())
        {
            return NotFound(new GlobalResponse<object>
            {
                Success = false,
                Message = "No Game found.",
                Data = null
            });
        }
        else
        {
            var resultList = new List<object>();
            var players = _mainService.GetPlayersInfo();

            if (players.Count == 0)
            {
                _logger?.Error("No players found.");
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = "No players found.",
                    Data = null
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
            return Ok(new GlobalResponse<List<object>>
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse<List<IShip>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse<object>))]
    public IActionResult GetShips([FromQuery] GetShipsRequest request)
    {
        if (!_mainService.IsGameInitialized())
        {
            return NotFound(new GlobalResponse<object>
            {
                Success = false,
                Message = "No Game found.",
                Data = null
            });
        }
        else
        {
            try
            {
                var players = _mainService.GetPlayersInfo().FirstOrDefault(player => player.Name.ToLower() == request.PlayerName.ToLower());
                if (players== null)
                {
                    _logger?.Error($"Player {request.PlayerName} not found.");
                    return NotFound(new GlobalResponse<object>
                    {
                        Success = false,
                        Message = $"Player {request.PlayerName} not found!",
                        Data = null
                    });
                }
                var ships = _mainService.GetPlayerShips(players);
                if (ships == null)
                {
                    _logger?.Error($"{request.PlayerName}'s ship(s) are not available or have already been deployed.");
                    return NotFound(new GlobalResponse<object>
                    {
                        Success = false,
                        Message = $"{request.PlayerName}'s ship(s) are not available or have already been deployed.",
                        Data = null
                    });
                }

                _logger?.Information($"Player {request.PlayerName}'s ships retrieved successfully");
                return Ok(new GlobalResponse<IReadOnlyList<IShip>>
                {
                    Success = true,
                    Message = $"Player {request.PlayerName}'s ships retrieved successfully.",
                    Data = ships
                });
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error retrieving {request.PlayerName}'s ships. {ex.Message}");
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Error retrieving {request.PlayerName}'s ships. {ex.Message}",
                    Data = null
                });
            }
        }
    }

    [HttpGet("board")]
    [SwaggerOperation(
        Summary = "Get the board of a player",
        Description = "Retrieves the board for a specific player",
        OperationId = "GetPlayerBoard"
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BoardResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse<object>))]
    public IActionResult GetBoard([FromQuery] GetBoardRequest request)
    {
        if (!_mainService.IsGameInitialized())
        {
            return NotFound(new GlobalResponse<object>
            {
                Success = false,
                Message = "No Game found.",
                Data = null
            });
        }
        else
        {
            try
            {
                var players = _mainService
                            .GetPlayersInfo()
                            .FirstOrDefault(player => 
                                player.Name.ToLower() == request.PlayerName.ToLower()
                            );
                if (players == null)
                {
                    _logger?.Error($"Player {request.PlayerName} not found!");
                    return NotFound(new GlobalResponse<object>
                    {
                        Success = false,
                        Message = $"Player {request.PlayerName} not found!",
                        Data = null
                    });
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
                return BadRequest(new GlobalResponse<List<object>>
                {
                    Success = false,
                    Message = $"Error retrieving {request.PlayerName}'s board. Error: {ex.Message}",
                    Data = null
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse<IShip>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse<object>))]
    public IActionResult PlaceShip([FromBody] PlaceShipRequest request)
    {
        if (!_mainService.IsGameInitialized())
        {
            return NotFound(new GlobalResponse<object>
            {
                Success = false,
                Message = "No Game found.",
                Data = null
            });
        }
        else
        {
            try
            {
                var players = _mainService
                            .GetPlayersInfo()
                            .FirstOrDefault(player => 
                                player.Name.ToLower() == request.PlayerName.ToLower()
                            );
                if (players == null)
                {
                    _logger?.Error($"Player {request.PlayerName} not found!");
                    return NotFound(new GlobalResponse<object>
                    {
                        Success = false,
                        Message = $"Player {request.PlayerName} not found!",
                        Data = null
                    });
                }

                var ships = _mainService
                            .GetPlayerShips(players)
                            .FirstOrDefault(ship => 
                                ship.Length == request.ShipLength
                            );
                if (ships == null)
                {
                    _logger?.Error($"Ship with length {request.ShipLength} is not available or has already been placed");
                    return NotFound(new GlobalResponse<object>
                    {
                        Success = false,
                        Message = $"Ship with length {request.ShipLength} is not available or has already been placed",
                        Data = null
                    });
                }

                var placeShip = _mainService.PlaceShips(players, ships, request.Start, request.End);

                if (!placeShip.Success)
                {
                    _logger?.Error(placeShip.Error);
                    return BadRequest(new GlobalResponse<object>
                    {
                        Success = false,
                        Message = placeShip.Error,
                        Data = null
                    });
                }

                _logger?.Information(placeShip.Value.ToString()!);
                return Ok(new GlobalResponse<IShip>
                {
                    Success = true,
                    Message = placeShip.Value.ToString()!,
                    Data = ships
                });
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error during placing ship: {ex.Message}");
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Error during placing ship: {ex.Message}",
                    Data = null
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse<object>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse<object>))]
    public IActionResult RemoveShip([FromBody] RemoveShipRequest request)
    {
        if (!_mainService.IsGameInitialized())
        {
            return NotFound(new GlobalResponse<object>
            {
                Success = false,
                Message = "No Game found.",
                Data = null
            });
        }
        else
        {
            try
            {
                var players = _mainService
                            .GetPlayersInfo()
                            .FirstOrDefault(player => 
                                player.Name.ToLower() == request.PlayerName.ToLower()
                            );
                if (players == null)
                {
                    _logger?.Error($"Player {request.PlayerName} not found!");
                    return NotFound(new GlobalResponse<object>
                    {
                        Success = false,
                        Message = $"Player {request.PlayerName} not found!",
                        Data = null
                    });
                }

                var ships = _mainService
                            .GetPlayerShips(players)
                            .FirstOrDefault(ship => 
                                ship.Length == request.ShipLength && ship.Positions != null
                            );
                if (ships == null)
                {
                    _logger?.Error($"Ship with length {request.ShipLength} is not available");
                    return NotFound(new GlobalResponse<object>
                    {
                        Success = false,
                        Message = $"Ship with length {request.ShipLength} is not available",
                        Data = null
                    });
                }

                _mainService.RemoveShip(players, ships);

                _logger?.Information($"Player {players.Name}'s Ship length {request.ShipLength} has been removed.");
                return Ok(new GlobalResponse<object>
                {
                    Success = true,
                    Message = $"Ship with length {request.ShipLength} successfully removed",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error during removing ship. {ex.Message}");
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Error during removing ship. {ex.Message}",
                    Data = null
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
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse<object>))]
    public async Task<IActionResult> Attack([FromBody] AttackRequest request)
    {
        if (!_mainService.IsGameInitialized())
        {
            return NotFound(new GlobalResponse<object>
            {
                Success = false,
                Message = "No Game found.",
                Data = null
            });
        }
        else
        {
            try
            {
                var coordinate = _mainService.CoordinateInput(request.Coordinate);
                var result = await _mainService.Attack(coordinate);

                if (!result.Success)
                {
                     _logger?.Error($"Error Result Attack: {result.Error}");
                    return BadRequest(new GlobalResponse<object>
                    {
                        Success = false,
                        Message = $"Error Result Attack: {result.Error}",
                        Data = null
                    });
                }

                var scores = _mainService.GetAllPlayerScore()
                            .ToDictionary(x => x.Key.Name, x => x.Value);

                var attackResult = result.Value;

                return Ok(new AttackResponse
                {
                    IsHit = attackResult.HumanHit,
                    ComputerHit = attackResult.ComputerHit,
                    ComputerShot = $"{(char)(attackResult.Coordinate.Col + 'A')}{attackResult.Coordinate.Row + 1}",
                    IsGameOver = attackResult.IsGameOver,
                    Message = attackResult.HumanHit ? "You hit the enemy ship" : "You missed",
                    Scores = scores
                });
                
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error during attack. {ex.Message}");
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Error during attack. Error : {ex.Message}",
                    Data = null
                });
            }
        }
    }
}