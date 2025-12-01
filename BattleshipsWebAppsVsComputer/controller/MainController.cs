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
            var initiateGame = _mainService.InitializeGame(request);
            if (!initiateGame.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Failed to start the game: {initiateGame.Error}",
                    Data = null
                });
            }

            return Ok(new GlobalResponse<object>
            {
                Success = true,
                Message = initiateGame.Value.ToString()!,
                Data = null
            });  
        }
        catch (Exception ex)
        {
            _logger?.Error($"[INITIALIZE-GAME] Error initializing game: {ex.Message}");
            return BadRequest(new GlobalResponse<object>
            {
                Success = false,
                Message = $"Errors when initializing game: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpGet("reset-game")]
    [SwaggerOperation(
        Summary = "Reset a Battleships game",
        Description = "Reset a game and remove all state",
        OperationId = "ResetGame"
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse<object>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse<object>))]
    public IActionResult ResetGame()
    {
        try
        {
            var resetGame = _mainService.ResetGane();
            if (!resetGame.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"{resetGame.Error}",
                    Data = null
                });
            }

            return Ok(new GlobalResponse<object>
            {
                Success = true,
                Message = resetGame.Value.ToString()!,
                Data = null
            });  
        }
        catch (Exception ex)
        {
            _logger?.Error($"[RESET-GAME] Error initializing game: {ex.Message}");
            return BadRequest(new GlobalResponse<object>
            {
                Success = false,
                Message = $"Errors when resetting game: {ex.Message}",
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
        try
        {
            var players = _mainService.GetPlayersInfo();
            if (!players.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Failed to get players: {players.Error}",
                    Data = null
                });
            }

            return Ok(new GlobalResponse<List<PlayerResponse>>
            {
                Success = true,
                Message = "Players retrieved successfully.",
                Data = players.Value.Select(player => new PlayerResponse { Name = player.Name }).ToList() // select Name aja
            });
        }
        catch (Exception ex)
        {
            _logger?.Error($"[GETPLAYERS] Error Get Players info: {ex.Message}");
            return BadRequest(new GlobalResponse<List<object>>
            {
                Success = true,
                Message = $"Error when getting Players info: {ex.Message}",
                Data = null
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
        try
        {
            var playerScore = _mainService.GetPlayerScore();
            if (!playerScore.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Failed to get scores: {playerScore.Error}",
                    Data = null
                });
            }

            return Ok(new GlobalResponse<Dictionary<string, int>>
            {
                Success = true,
                Message = "Scores retrieved successfully.",
                Data = playerScore.Value
            });
        }
        catch (Exception ex)
        {
            _logger?.Error($"[GETSCORES] Error Get Scores game: {ex.Message}");
            return BadRequest(new GlobalResponse<object>
            {
                Success = true,
                Message = $"Error when getting Scores game: {ex.Message}",
                Data = null
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
        try
        {
            var getPlayers = _mainService.GetPlayersInfo();
            if (!getPlayers.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Failed to get players: {getPlayers.Error}",
                    Data = null
                });
            }

            var players = getPlayers.Value
                        .FirstOrDefault(
                            player => player.Name.ToLower() == request.PlayerName.ToLower()
                        );

            var getShips = _mainService.GetPlayerShips(players!);
            if (!getShips.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Failed to get ships: {getShips.Error}",
                    Data = null
                });
            }

            return Ok(new GlobalResponse<IReadOnlyList<IShip>>
            {
                Success = true,
                Message = $"Player {request.PlayerName}'s ships retrieved successfully.",
                Data = getShips.Value
            });
        }
        catch (Exception ex)
        {
            _logger?.Error($"[GETSHIPS] Error retrieving {request.PlayerName}'s ships. {ex.Message}");
            return BadRequest(new GlobalResponse<object>
            {
                Success = false,
                Message = $"Error when retrieving {request.PlayerName}'s ships. {ex.Message}",
                Data = null
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
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse<object>))]
    public IActionResult GetBoard([FromQuery] GetBoardRequest request)
    {
        try
        {
            var getPlayers = _mainService.GetPlayersInfo();
            if (!getPlayers.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Failed to get players: {getPlayers.Error}",
                    Data = null
                });
            }

            var players = getPlayers.Value
                        .FirstOrDefault(
                            player => player.Name.ToLower() == request.PlayerName.ToLower()
                        );

            var board = _mainService.GetBoardInfo(players!);
            if (!board.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Failed to get board: {board.Error}",
                    Data = null
                });
            }

            return Ok(board.Value);
        }
        catch (Exception ex)
        {
            _logger?.Error($"[GETBOARD] Error retrieving {request.PlayerName}'s board. {ex.Message}");
            return BadRequest(new GlobalResponse<List<object>>
            {
                Success = false,
                Message = $"Error when retrieving {request.PlayerName}'s board. Error: {ex.Message}",
                Data = null
            });
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
        try
        {
            var getPlayers = _mainService.GetPlayersInfo();
            if (!getPlayers.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Failed to get players: {getPlayers.Error}",
                    Data = null
                });
            }

            var players = getPlayers.Value
                        .FirstOrDefault(
                            player => player.Name.ToLower() == request.PlayerName.ToLower()
                        );

            var getShips = _mainService.GetPlayerShips(players!);
            if (!getShips.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Failed to get ships: {getShips.Error}",
                    Data = null
                });
            }

            var ships = getShips.Value
                        .FirstOrDefault(ship => 
                            ship.Length == request.ShipLength && (ship.Positions == null || !ship.Positions.Any())
                        );

            var placeShip = _mainService.PlaceShips(players!, ships!, request.Start, request.End);

            if (!placeShip.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = placeShip.Error,
                    Data = null
                });
            }

            return Ok(new GlobalResponse<IShip>
            {
                Success = true,
                Message = placeShip.Value.ToString()!,
                Data = ships
            });
        }
        catch (Exception ex)
        {
            _logger?.Error($"[PLACESHIP] Error during placing ship: {ex.Message}");
            return BadRequest(new GlobalResponse<object>
            {
                Success = false,
                Message = $"Error during placing ship: {ex.Message}",
                Data = null
            });
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
                var getPlayers = _mainService.GetPlayersInfo();
                if (!getPlayers.Success)
                {
                    return BadRequest(new GlobalResponse<object>
                    {
                        Success = false,
                        Message = $"Failed to get players: {getPlayers.Error}",
                        Data = null
                    });
                }

                var players = getPlayers.Value
                            .FirstOrDefault(
                                player => player.Name.ToLower() == request.PlayerName.ToLower()
                            );

                var getShips = _mainService.GetPlayerShips(players!);
                if (!getShips.Success)
                {
                    return BadRequest(new GlobalResponse<object>
                    {
                        Success = false,
                        Message = $"Failed to get ships: {getShips.Error}",
                        Data = null
                    });
                }

                var ships = getShips.Value
                            .FirstOrDefault(ship => 
                                ship.Length == request.ShipLength && ship.Positions != null
                            );

                var removeShip = _mainService.RemoveShip(players!, ships!);
                if (!removeShip.Success)
                {
                    return BadRequest(new GlobalResponse<object>
                    {
                        Success = false,
                        Message = removeShip.Error,
                        Data = null
                    });
                }

                return Ok(new GlobalResponse<object>
                {
                    Success = true,
                    Message = $"Ship with length {request.ShipLength} successfully removed",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger?.Error($"[REMOVESHIP] Error during removing ship: {ex.Message}");
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Error during removing ship: {ex.Message}",
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
        try
        {
            var checkCoordinate = _mainService.CoordinateInput(request.Coordinate);
            if (!checkCoordinate.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Error coordinate: {checkCoordinate.Error}",
                    Data = null
                });
            }
            var result = await _mainService.Attack(checkCoordinate.Value);
            if (!result.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Error Result Attack: {result.Error}",
                    Data = null
                });
            }

            var playerScore = _mainService.GetPlayerScore();
            if (!playerScore.Success)
            {
                return BadRequest(new GlobalResponse<object>
                {
                    Success = false,
                    Message = $"Failed to get scores: {playerScore.Error}",
                    Data = null
                });
            }

            var attackResult = result.Value;

            return Ok(new AttackResponse
            {
                IsHit = attackResult.HumanHit,
                ComputerHit = attackResult.ComputerHit,
                ComputerShot = $"{(char)(attackResult.Coordinate.Col + 'A')}{attackResult.Coordinate.Row + 1}",
                IsGameOver = attackResult.IsGameOver,
                Message = attackResult.HumanHit ? "You hit the enemy ship" : "You missed",
                Scores = playerScore.Value
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