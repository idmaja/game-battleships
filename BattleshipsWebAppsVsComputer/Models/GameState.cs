public class GameState : IGameState
{
    public List<IPlayer> Players { get; set; } = new();
    public Dictionary<IPlayer, int> PlayerScores { get; set; } = new();
    public Dictionary<IPlayer, IBoard> PlayerBoard { get; set; } = new();
    public Dictionary<IPlayer, List<IShip>> PlayerShips { get; set; } = new();
}