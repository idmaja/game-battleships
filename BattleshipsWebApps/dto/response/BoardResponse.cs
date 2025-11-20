public class BoardResponse
{
    public int Width { get; set; }
    public int Height { get; set; }
    public List<CellResponse> Cells { get; set; } = new();
}