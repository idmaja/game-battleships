public class AttackRequest 
{
    public bool HumanHit { get; set; }
    public bool ComputerHit { get; set; }
    public string Coordinate { get; set; } = "";
    public bool IsGameOver { get; set; }
    
}