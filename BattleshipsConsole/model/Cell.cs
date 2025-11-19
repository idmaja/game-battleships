public interface ICell
{
    IShip? Ship { get; set; }
    Coordinate Coordinate { get; set; }
}

public class Cell : ICell
{
    public IShip? Ship { get; set; }
    public Coordinate Coordinate { get; set; }

    public Cell(Coordinate coordinate, IShip? ship = null)
    {
        Ship = ship;
        Coordinate = coordinate;
    }

}