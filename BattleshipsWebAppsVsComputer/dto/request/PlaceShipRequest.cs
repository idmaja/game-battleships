public class PlaceShipRequest
{
    public string PlayerName { get; set; } = string.Empty;
    public int ShipLength { get; set; }  
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;   
}