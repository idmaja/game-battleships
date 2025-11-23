public class AttackResponse 
{
    public bool IsHit { get; set; }

    public bool IsGameOver { get; set; }

    public string Message { get; set; } = "";

    public Dictionary<string, int>? Scores { get; set; }

    public string ComputerShot { get; set; } = ""; 

    public bool ComputerHit { get; set; }
}