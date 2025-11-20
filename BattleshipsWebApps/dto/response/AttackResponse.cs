public class AttackResponse 
{
    public bool IsHit { get; set; }
    public bool IsGameOver { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, int> Scores { get; set; } = new();
}