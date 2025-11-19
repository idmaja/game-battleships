public interface IPlayer
{
    string Name { get; set; }
    int Score { get; internal set; } // internal agar score dirubah oleh game nya sendiri
}

public class Player : IPlayer
{
    public string Name { get; set; }
    public int Score { get; set; }

    public Player(string name)
    {
        Name = name;
        Score = 0;
    }
}