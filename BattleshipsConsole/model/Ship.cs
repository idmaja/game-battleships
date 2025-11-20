public class Ship : IShip
{
    public int Length { get; set; }
    public int Hits { get; set; } = 0;
    public bool IsSunk { get; set; }

    public List<Coordinate> Positions { get; set; } = new List<Coordinate>();

    public Ship(int length, int hits, bool isSunk, List<Coordinate> positions)
    {
        Length = length;
        Hits = hits;
        IsSunk = isSunk;
        Positions = positions ?? new List<Coordinate>();
    }
}