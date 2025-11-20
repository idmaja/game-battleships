using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/battleships")]
public class MainController : ControllerBase
{
    private readonly IMainService _mainService;

    public MainController(IMainService mainService)
    {
        _mainService = mainService;
    }

    [HttpGet("players")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse))]
    public IActionResult GetPlayers()
    {
        var players = _mainService
            .GetPlayersInfo()
            .Select(p => new { Name = p.Name })
            .ToList();

        return Ok(new
        {
            Success = true,
            Data = players
        });
    }

    [HttpGet("scores")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse))]
    public IActionResult GetScores()
    {
        var result = new Dictionary<string, int>();

        foreach (var p in _mainService.GetPlayersInfo())
        {
            result[p.Name] = _mainService.GetPlayerScore(p);
        }

        return Ok(new
        {
            Success = true,
            Data = result
        });
    }

    [HttpGet("ships/{playerName}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse))]
    public IActionResult GetShips(string playerName)
    {
        var players = _mainService.GetPlayersInfo();
        var player = players.FirstOrDefault(p => p.Name == playerName);
        if (player == null)
        {
            return NotFound(new GlobalResponse
            {
                Success = false,
                Message = $"Player {playerName} not found!"
            });
        }
        var ships = _mainService.GetPlayerShips(player);
        if (ships == null)
        {
            return NotFound(new GlobalResponse
            {
                Success = false,
                Message = $"{playerName}'s ship(s) are not available or have already been deployed."
            });
        }
        
        return Ok(new
        {
            Success = true,
            Data = ships
        });
    }

    [HttpPost("place-ship")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GlobalResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(GlobalResponse))]
    public IActionResult PlaceShip([FromBody] PlaceShipRequest request)
    {
        var players = _mainService.GetPlayersInfo();
        var player = players.FirstOrDefault(p => p.Name == request.PlayerName);
        if (player == null)
        {
            return NotFound(new GlobalResponse
            {
                Success = false,
                Message = $"Player {request.PlayerName} not found!"
            });
        }

        var ships = _mainService.GetPlayerShips(player);
        var ship = ships.FirstOrDefault(s => s.Length == request.ShipLength);
        if (ship == null)
        {
            return NotFound(new GlobalResponse
            {
                Success = false,
                Message = $"Ship with length {request.ShipLength} is not available or has already been placed"
            });
        }

        bool success = _mainService.PlaceShips(player, ship, request.Start, request.End);

        if (!success)
        {
            return BadRequest(new GlobalResponse
            {
                Success = false,
                Message = "Ship placement failed. Check the coordinates and length of the ship!"
            });
        }

        return Ok(new GlobalResponse
        {
            Success = true,
            Message = "Ship successfully placed"
        });
    }

    [HttpPost("attack")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AttackResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(AttackResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(AttackResponse))]
    public IActionResult Attack([FromBody] AttackRequest request)
    {
        Coordinate coord = _mainService.CoordinateInput(request.Coordinate);

        bool isHit = _mainService.Attack(coord);

        // ambil skor per player, konversi ke dictionary Name -> Score
        var scores = new Dictionary<string, int>();
        foreach (var p in _mainService.GetPlayersInfo())
        {
            scores[p.Name] = _mainService.GetPlayerScore(p);
        }

        bool isGameOverForDefender = _mainService
            .GetPlayersInfo()
            .Any(p => _mainService.IsAllShipsSunk(p));

        return Ok(new AttackResponse
        {
            IsHit = isHit,
            IsGameOver = isGameOverForDefender,
            Message = isHit ? "Hit" : "Miss",
            Scores = scores
        });
    }

    [HttpGet("board/{playerName}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BoardResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BoardResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BoardResponse))]
    public IActionResult GetBoard(string playerName)
    {
        var player = _mainService.GetPlayersInfo().FirstOrDefault(p => p.Name == playerName);
        if (player == null)
            return NotFound($"Player {playerName} not found!");

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

        return Ok(boardResponse);
    }
}