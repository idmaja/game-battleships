public interface IBoard
{
    int Height { get; set; }
    int Width { get; set; }
    ICell[,] Cells { get; set; }
}

public class Board : IBoard
{
    public int Height { get; set; }
    public int Width { get; set; }
    public ICell[,] Cells { get; set; }

    public Board(int height, int width)
    {
        Height = height;
        Width = width;
        Cells = new Cell[height, width];

        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                Cells[row, column] = new Cell(new Coordinate(row, column));
            }
        }
    }
}