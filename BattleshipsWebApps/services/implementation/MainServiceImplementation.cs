public class MainService : IMainService
{
    private List<IPlayer> _players;
    private Dictionary<IPlayer, int> _playerScores;
    private Dictionary<IPlayer, IBoard> _playerBoards;
    private Dictionary<IPlayer, List<IShip>> _playerShips;
    private int _currentPlayerIndex;

    public event Action<IPlayer, IPlayer, Dictionary<IPlayer, int>>? OnMessageResult;

    public MainService(IPlayer player1, IPlayer player2, IBoard board1, IBoard board2, List<IShip> ship1, List<IShip> ship2)
    {
        _players = new List<IPlayer> { player1, player2 };
        _playerBoards = new Dictionary<IPlayer, IBoard> { {player1, board1}, {player2, board2} };
        _playerShips = new Dictionary<IPlayer, List<IShip>> { {player1, ship1}, {player2, ship2} };
        _playerScores = new Dictionary<IPlayer, int> { {player1, 0}, {player2, 0} };
        _currentPlayerIndex = 0;
    }
    public bool PlaceShips(IPlayer player, IShip ship, string coorStart, string coorEnd)
    {
        var board = _playerBoards[player];

        Coordinate start = CoordinateInput(coorStart);
        Coordinate end = CoordinateInput(coorEnd);

        if (start.Row < 0 || start.Row >= board.Height ||
            end.Row < 0 || end.Row >= board.Height ||
            start.Col < 0 || start.Col >= board.Width ||
            end.Col < 0 || end.Col >= board.Width)
        {
            Console.WriteLine("Invalid ship placement. Ship is outside the board.");
            return false;
        }
        
        bool isHorizontal = start.Row == end.Row;
        bool isVertical = start.Col == end.Col;


        if (!isHorizontal && !isVertical)
        {
            Console.WriteLine("Invalid ship placement. Ships must be placed horizontally or vertically.");
            return false;
        }

        int distance;

        if (isHorizontal) 
            distance = Math.Abs(end.Col - start.Col) + 1;
        else
            distance = Math.Abs(end.Row - start.Row) + 1;

        if (distance != ship.Length)
        {
            Console.WriteLine($"Ship length must be {ship.Length}");
            return false;
        }

        List<Coordinate> coordinates = CheckShipPath(start, end);

        foreach (var coordinate in coordinates)
        {
            if (board.Cells[coordinate.Row, coordinate.Col].Ship != null)
            {
                Console.WriteLine("Invalid ship placement. Ships cannot overlap or extend beyond the board.");
                return false;
            }
        }

        foreach (var coordinate in coordinates)
            board.Cells[coordinate.Row, coordinate.Col].Ship = (Ship?)ship;

        ship.Positions = coordinates;

        Console.WriteLine($"Ship placed successfully!, {coordinates}, ship: {ship.Positions}, Length: {ship.Length}");
        return true;
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

    public bool Attack(Coordinate coordinate)
    {
        IPlayer attacker = GetCurrentPlayer();
        IPlayer defender = NextPlayer();

        bool isShipHit = ReceivedAttack(defender, coordinate);

        if (isShipHit)
        {
            IncreasePlayerScore(attacker);
            if (IsAllShipsSunk(defender)) 
                ResultMessage(attacker, defender, GetAllPlayerScore());
        }
        else 
            NextPlayer();
        
        return isShipHit;
    }

    public bool ReceivedAttack(IPlayer defender, Coordinate coordinate)
    {
        var boardDefender = _playerBoards[defender];
        var cell = boardDefender.Cells[coordinate.Row, coordinate.Col];

        if (cell.Ship == null) 
            return false;

        IncreaseShipHit(cell);
        
        if (cell.Ship.Hits >= cell.Ship.Length) 
            cell.Ship.IsSunk = true;

        return true;
    }

    public bool IsAllShipsSunk(IPlayer defender)
    {
        var shipsDefender = _playerShips[defender];

        return shipsDefender.All(ship => ship.IsSunk);
    }

    public void IncreasePlayerScore(IPlayer player) => _playerScores[player]++;
    public void IncreaseShipHit(ICell cell) => cell.Ship!.Hits++;

    public virtual void ResultMessage(IPlayer attacker, IPlayer defender, Dictionary<IPlayer, int> playerScore) 
        => OnMessageResult?.Invoke(attacker, defender, _playerScores);

    public IPlayer GetCurrentPlayer() => _players[_currentPlayerIndex];
    public IPlayer NextPlayer() => _players[(_currentPlayerIndex + 1) % _players.Count];
    public int GetPlayerScore(IPlayer player) => _playerScores[player];
    public Dictionary<IPlayer, int> GetAllPlayerScore() => _playerScores;
    public IBoard GetBoardInfo(IPlayer player) => _playerBoards[player];
    public IReadOnlyList<IPlayer> GetPlayersInfo() => _players;
    public IReadOnlyDictionary<IPlayer, IBoard> GetPlayerBoards() => _playerBoards;
    public IReadOnlyList<IShip> GetPlayerShips(IPlayer player) => _playerShips[player];
}