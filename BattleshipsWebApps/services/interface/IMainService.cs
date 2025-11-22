public interface IMainService
{
    bool PlaceShips(IPlayer player, IShip ship, string coorStart, string coorEnd, out string message);
    List<Coordinate> CheckShipPath(Coordinate coorStart, Coordinate coorEnd);
    Coordinate CoordinateInput(string input);
    Task<bool> Attack(Coordinate coordinate);
    bool ReceivedAttack(IPlayer defender, Coordinate coordinate, out string message);
    bool IsAllShipsSunk(IPlayer defender);
    void IncreasePlayerScore(IPlayer player);
    void IncreaseShipHit(ICell cell);
    IPlayer GetCurrentPlayer();
    IPlayer NextPlayer();
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

    // Event
    event Action<string>? OnMessageReceived;
}