public interface IGameState
{
    List<IPlayer> Players { get; set; }
    Dictionary<IPlayer, int> PlayerScores { get; set; }
    Dictionary<IPlayer, IBoard> PlayerBoard { get; set; }
    Dictionary<IPlayer, List<IShip>> PlayerShips { get; set; }
}