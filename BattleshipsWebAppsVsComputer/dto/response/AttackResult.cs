public class AttackResult
{
    public bool HumanHit { get; set; }
    public bool ComputerHit { get; set; }
    public Coordinate Coordinate { get; set; } = new Coordinate(0, 0);
    public bool IsGameOver { get; set; }
}
