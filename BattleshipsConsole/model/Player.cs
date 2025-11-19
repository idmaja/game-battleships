public interface IPlayer
{
    string Name { get; }
}

public class Player : IPlayer
{
    public string Name { get; set; }

    public Player(string name)
    {
        Name = name;
    }
}