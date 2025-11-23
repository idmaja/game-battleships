public interface IShip
{
    int Length { get; set;}
    int Hits { get; set;}
    bool IsSunk { get; set; }

    List<Coordinate> Positions { get; set; }
}