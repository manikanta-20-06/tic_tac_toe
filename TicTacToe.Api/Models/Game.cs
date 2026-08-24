namespace TicTacToe.Api.Models;

public class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Player?[,] Board { get; set; } = new Player?[3, 3];
    public Player CurrentPlayer { get; set; } = Player.X;
    public GameMode GameMode { get; set; }
    public Difficulty Difficulty { get; set; } = Difficulty.Medium;
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public Player? Winner { get; set; }
    public List<int[]> WinningCells { get; set; } = new();
    public List<Move> MoveHistory { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool ScoreboardUpdated { get; set; }

    public Game() { }

    public Game(GameMode mode, Difficulty difficulty = Difficulty.Medium)
    {
        Id = Guid.NewGuid();
        Board = new Player?[3, 3];
        CurrentPlayer = Player.X;
        GameMode = mode;
        Difficulty = difficulty;
        Status = GameStatus.InProgress;
        Winner = null;
        WinningCells = new List<int[]>();
        MoveHistory = new List<Move>();
        CreatedAt = DateTime.UtcNow;
        ScoreboardUpdated = false;
    }
}
