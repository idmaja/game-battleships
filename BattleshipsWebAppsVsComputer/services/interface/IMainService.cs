public interface IMainService
{
    bool PlaceShips(IPlayer player, IShip ship, string coorStart, string coorEnd, out string message);
    List<Coordinate> CheckShipPath(Coordinate coorStart, Coordinate coorEnd);
    Coordinate CoordinateInput(string input);
    Task<AttackResult> Attack(Coordinate coordinate);
    bool ReceivedAttack(IPlayer defender, Coordinate coordinate, out string message);
    bool IsAllShipsSunk(IPlayer defender);
    void IncreasePlayerScore(IPlayer player);
    void IncreaseShipHit(ICell cell);
    int GetPlayerScore(IPlayer player);
    Dictionary<IPlayer, int> GetAllPlayerScore();
    IBoard GetBoardInfo(IPlayer player);
    IReadOnlyList<IPlayer> GetPlayersInfo();
    IReadOnlyDictionary<IPlayer, IBoard> GetPlayerBoards();
    IReadOnlyList<IShip> GetPlayerShips(IPlayer player);
    Task MessageNotification(string message);

    // New Methods
    bool IsGameInitialized();
    void InitializeGame(CreateGameRequest request);
    Serilog.ILogger GetLogger();
    Coordinate GetRandomShotForComputer(IPlayer human);
    void RemoveShip(IPlayer player, IShip ship);

    // Event
    event Action<string>? OnMessageReceived;
}