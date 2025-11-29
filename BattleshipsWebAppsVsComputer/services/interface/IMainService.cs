public interface IMainService
{
    Result<object> PlaceShips(IPlayer player, IShip ship, string coorStart, string coorEnd);
    Result<List<Coordinate>> PlaceShipInPath(Coordinate coorStart, Coordinate coorEnd);
    Result<Coordinate> CoordinateInput(string input);
    Task<Result<AttackResult>> Attack(Coordinate coordinate);
    bool ReceivedAttack(IPlayer defender, Coordinate coordinate, out string message);
    bool IsAllShipsSunk(IPlayer defender);
    void IncreasePlayerScore(IPlayer player);
    void IncreaseShipHit(ICell cell);
    Result<Dictionary<string, int>> GetPlayerScore();
    Result<BoardResponse> GetBoardInfo(IPlayer player);
    Result<IReadOnlyList<IPlayer>> GetPlayersInfo();
    Result<IReadOnlyList<IShip>> GetPlayerShips(IPlayer player);
    Task MessageNotification(string message);

    // New Methods
    bool IsGameInitialized();
    Result<object> InitializeGame(CreateGameRequest request);
    Result<object> ResetGane();
    Coordinate GetRandomShotForComputer(IPlayer human);
    Result<object> RemoveShip(IPlayer player, IShip ship);

    // Event
    event Action<string>? OnGameResult;

    // UNUSED SERVICE(S)
    // Dictionary<IPlayer, int> GetAllPlayerScore();
    // IReadOnlyDictionary<IPlayer, IBoard> GetPlayerBoards();
}