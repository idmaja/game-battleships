public interface IShip
{
    int Length { get; set;}
    int Hits { get; set;}
    bool IsSunk { get; set; }

    List<Coordinate> Positions { get; set; }
}

public class Ship : IShip
{
    public int Length { get; set;}
    public int Hits { get; set;}
    public bool IsSunk { get; set; }

    public List<Coordinate> Positions { get; set; }

    public Ship(int length)
    {
        Length = length;
        Hits = 0;
        Positions = new List<Coordinate>();
    }
}