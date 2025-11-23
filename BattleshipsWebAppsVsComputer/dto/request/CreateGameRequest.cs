public class CreateGameRequest
{
    public string PlayerName { get; set; } = string.Empty;
    public string ComputerName { get; set; } = string.Empty;
    public int BoardWidth { get; set; }
    public int BoardHeight { get; set; }
    public List<int> ShipLengthsPlayer { get; set; } = new();
    public List<int> ShipLengthsComputer { get; set; } = new();
}