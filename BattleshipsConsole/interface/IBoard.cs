public interface IBoard
{
    int Height { get; set; }
    int Width { get; set; }
    ICell[,] Cells { get; }
}