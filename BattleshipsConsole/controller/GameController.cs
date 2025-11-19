public class GameController
{
    private List<IPlayer> _players;
    private Dictionary<IPlayer, IBoard> _playerBoards;
    private Dictionary<IPlayer, List<IShip>> _playerShips;
    private int _currentPlayerIndex;
    private int _boardHeight;
    private int _boardWidth;

    public Action<IPlayer, IPlayer>? OnMessageResult;

    public GameController(IPlayer player1, IPlayer player2)
    {
        _boardHeight = 10; _boardWidth = 10;
        _players = new List<IPlayer> { player1, player2 };
        _playerBoards = new Dictionary<IPlayer, IBoard>
        {
            { player1, new Board(_boardHeight, _boardWidth) },
            { player2, new Board(_boardHeight, _boardWidth) }
        };
        _playerShips = new Dictionary<IPlayer, List<IShip>>
        {
            { player1, new List<IShip>() },
            { player2, new List<IShip>() }
        };
        _currentPlayerIndex = 0;
    }

    public IPlayer CurrentPlayer => _players[_currentPlayerIndex];
    public IPlayer NextPlayer => _players[(_currentPlayerIndex + 1) % _players.Count];

}