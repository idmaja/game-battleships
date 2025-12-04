public class Board : IBoard
{
    public int Height { get; set; }
    public int Width { get; set; }
    public ICell[,] Cells { get; }

    public Board(int height, int width, ICell[,]? cells = null)
    {
        Height = height;
        Width = width;

        if (cells != null)
        {
            Cells = cells;
        }
        else
        {
            Cells = new Cell[Height, Width];
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    Cells[row, column] = new Cell(new Coordinate(row, column));
                }
            }
        }
    }
}