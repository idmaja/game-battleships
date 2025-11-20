public interface ICell
{
    IShip? Ship { get; set; }
    Coordinate Coordinate { get; set; }
    bool IsHit { get; }
}