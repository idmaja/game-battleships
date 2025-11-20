public class GameController
{

    private List<IPlayer> _players;
    private Dictionary<IPlayer, int> _playerScores;
    private Dictionary<IPlayer, IBoard> _playerBoards;
    private Dictionary<IPlayer, List<IShip>> _playerShips;
    private int _currentPlayerIndex;

    public event Action<IPlayer, IPlayer, Dictionary<IPlayer, int>>? OnMessageResult;

    public GameController(IPlayer player1, IPlayer player2, IBoard board1, IBoard board2, List<IShip> ship1, List<IShip> ship2)
    {
        _players = new List<IPlayer> { player1, player2 };
        _playerBoards = new Dictionary<IPlayer, IBoard> { {player1, board1}, {player2, board2} };
        _playerShips = new Dictionary<IPlayer, List<IShip>> { {player1, ship1}, {player2, ship2} };
        _playerScores = new Dictionary<IPlayer, int> { {player1, 0}, {player2, 0} };
        _currentPlayerIndex = 0;
    }


    public void StartGame()
    {
        // simpan koordinat hit per player
        var hitCoordinates = new Dictionary<IPlayer, HashSet<Coordinate>>();
        foreach (var player in _players)
        {
            hitCoordinates[player] = new HashSet<Coordinate>();
        }

        // gambar board saat fase placement (cuma ship dan kosong)
        void DrawPlacementBoard(IBoard board)
        {
            // header kolom
            Console.Write("   ");
            for (int col = 0; col < board.Width; col++)
            {
                char letter = (char)('A' + col);
                Console.Write($" {letter} ");
            }
            Console.WriteLine();

            for (int row = 0; row < board.Height; row++)
            {
                // nomor baris
                Console.Write($"{row + 1,2} ");

                for (int col = 0; col < board.Width; col++)
                {
                    var cell = board.Cells[row, col];

                    if (cell.Ship != null)
                        Console.Write("[s]");
                    else
                        Console.Write("[]");
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        // gambar board saat battle (s, x, dan kosong)
        void DrawBattleBoard(IBoard board, HashSet<Coordinate> hits)
        {
            Console.Write("   ");
            for (int col = 0; col < board.Width; col++)
            {
                char letter = (char)('A' + col);
                Console.Write($" {letter} ");
            }
            Console.WriteLine();

            for (int row = 0; row < board.Height; row++)
            {
                Console.Write($"{row + 1,2} ");

                for (int col = 0; col < board.Width; col++)
                {
                    var coord = new Coordinate(row, col);
                    var cell = board.Cells[row, col];

                    if (hits.Contains(coord))
                        Console.Write("[x]");
                    else
                        Console.Write("[]");

                    Console.Write(" ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        Console.Clear();
        Console.WriteLine("Welcome to Battleships.");
        Console.WriteLine();

        // FASE 1: SETUP, MASANG KAPAL
        foreach (var player in _players)
        {
            var board = _playerBoards[player];
            var ships = _playerShips[player];

            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                bool placed = false;

                while (!placed)
                {
                    Console.Clear();
                    Console.WriteLine($"Player {player.Name} place your ships.");
                    Console.WriteLine($"Ship {i + 1} length {ship.Length}.");
                    Console.WriteLine("Current board.");
                    DrawPlacementBoard(board);

                    Console.Write("Start coordinate (ex A1): ");
                    var startInput = Console.ReadLine() ?? string.Empty;

                    Console.Write("End coordinate (ex A5): ");
                    var endInput = Console.ReadLine() ?? string.Empty;

                    placed = PlaceShips(player, ship, startInput, endInput);

                    if (!placed)
                    {
                        // Console.WriteLine("Placement failed!");
                        Console.WriteLine("Placement failed, press any key to try again.");
                        Console.ReadKey();
                    }
                }

                // sudah placed, board sudah update
                Console.Clear();
                Console.WriteLine($"Player {player.Name} place your ships.");
                Console.WriteLine($"Ship {i + 1} placed.");
                Console.WriteLine("Board after placement.");
                DrawPlacementBoard(board);

                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }

            Console.Clear();
            Console.WriteLine($"All ships placed for {player.Name}.");
            Console.WriteLine("Final board.");
            DrawPlacementBoard(board);
            Console.WriteLine("Press any key to continue to next player.");
            Console.ReadKey();
            Console.Clear();
        }

        Console.WriteLine("All ships placed. Battle starts.");
        Console.WriteLine();

        _currentPlayerIndex = 0;

        // FASE 2: BATTLE
        while (true)
        {
            var attacker = GetCurrentPlayer();
            var defenderIndex = (_currentPlayerIndex + 1) % _players.Count;
            var defender = _players[defenderIndex];

            Console.WriteLine($"Attacker : {attacker.Name}");
            Console.WriteLine($"Defender : {defender.Name}");
            Console.WriteLine();
            Console.WriteLine("Defender board.");
            DrawBattleBoard(_playerBoards[defender], hitCoordinates[defender]);

            Console.Write("Attack coordinate (ex A1): ");
            var input = Console.ReadLine() ?? string.Empty;
            var coordinate = CoordinateInput(input);

            bool hit = Attack(coordinate);

            if (hit)
            {
                Console.WriteLine("Hit.");
                hitCoordinates[defender].Add(coordinate);
            }
            else
            {
                Console.WriteLine("Miss.");
            }

            Console.WriteLine();
            Console.WriteLine("Defender board after attack.");
            DrawBattleBoard(_playerBoards[defender], hitCoordinates[defender]);

            if (IsAllShipsSunk(defender))
            {
                ResultMessage(attacker, defender, GetAllPlayerScore());
                break;
            }

            if (!hit)
            {
                _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
            }

            Console.WriteLine("Press any key for next turn.");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private bool PlaceShips(IPlayer player, IShip ship, string coorStart, string coorEnd)
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

    private List<Coordinate> CheckShipPath(Coordinate coorStart, Coordinate coorEnd)
    {
        
        List<Coordinate> coordinates = new List<Coordinate>();
        
        // if (coorStart.Row != coorEnd.Row && coorStart.Col != coorEnd.Col)
        // {
        //     Console.WriteLine("Ships cannot be placed diagonally.");
        //     return new List<Coordinate>();
        // }
        
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

    private Coordinate CoordinateInput(string input)
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

    private bool Attack(Coordinate coordinate)
    {
        IPlayer attacker = GetCurrentPlayer();
        IPlayer defender = NextPlayer();

        bool isShipHit = IsReceivedAttack(defender, coordinate);

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

    private bool IsReceivedAttack(IPlayer defender, Coordinate coordinate)
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

    private bool IsAllShipsSunk(IPlayer defender)
    {
        var shipsDefender = _playerShips[defender];

        return shipsDefender.All(ship => ship.IsSunk);
    }

    private void IncreasePlayerScore(IPlayer player) => _playerScores[player]++;
    private void IncreaseShipHit(ICell cell) => cell.Ship!.Hits++;

    protected virtual void ResultMessage(IPlayer attacker, IPlayer defender, Dictionary<IPlayer, int> playerScore) 
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