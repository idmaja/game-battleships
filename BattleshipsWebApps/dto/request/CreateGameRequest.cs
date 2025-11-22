public class CreateGameRequest
{
    public string Player1Name { get; set; } = string.Empty;
    public string Player2Name { get; set; } = string.Empty;
    public int BoardWidth { get; set; }
    public int BoardHeight { get; set; }
    public List<int> ShipLengthsPlayer1 { get; set; } = new();
    public List<int> ShipLengthsPlayer2 { get; set; } = new();
}