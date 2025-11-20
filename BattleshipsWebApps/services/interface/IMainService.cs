public interface IMainService
{
    bool PlaceShips(IPlayer player, IShip ship, string coorStart, string coorEnd);
    List<Coordinate> CheckShipPath(Coordinate coorStart, Coordinate coorEnd);
    Coordinate CoordinateInput(string input);
    bool Attack(Coordinate coordinate);
    bool ReceivedAttack(IPlayer defender, Coordinate coordinate);
    bool IsAllShipsSunk(IPlayer defender);
    void IncreasePlayerScore(IPlayer player);
    void IncreaseShipHit(ICell cell);
    void ResultMessage(IPlayer attacker, IPlayer defender, Dictionary<IPlayer, int> playerScore);
    IPlayer GetCurrentPlayer();
    IPlayer NextPlayer();
    int GetPlayerScore(IPlayer player);
    Dictionary<IPlayer, int> GetAllPlayerScore();
    IBoard GetBoardInfo(IPlayer player);
    IReadOnlyList<IPlayer> GetPlayersInfo();
    IReadOnlyDictionary<IPlayer, IBoard> GetPlayerBoards();
    IReadOnlyList<IShip> GetPlayerShips(IPlayer player);
}