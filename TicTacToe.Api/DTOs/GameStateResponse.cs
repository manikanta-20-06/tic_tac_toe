using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

public class GameStateResponse
{
    public Guid Id { get; set; }
    public string[][] Board { get; set; } = Array.Empty<string[]>();
    public string CurrentPlayer { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Winner { get; set; }
    public List<int[]> WinningCells { get; set; } = new();
    public List<MoveResponse> MoveHistory { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool CanUndo { get; set; }

    public static GameStateResponse FromGame(Game game)
    {
        return new GameStateResponse
        {
            Id = game.Id,
            Board = ConvertBoard(game.Board),
            CurrentPlayer = game.CurrentPlayer.ToString(),
            GameMode = game.GameMode.ToString(),
            Difficulty = game.Difficulty.ToString(),
            Status = game.Status.ToString(),
            Winner = game.Winner?.ToString(),
            WinningCells = game.WinningCells,
            MoveHistory = game.MoveHistory.Select(m => MoveResponse.FromMove(m)).ToList(),
            CreatedAt = game.CreatedAt,
            CanUndo = game.Status == GameStatus.InProgress && game.MoveHistory.Count > 0
        };
    }

    private static string[][] ConvertBoard(Player?[,] board)
    {
        var result = new string[3][];
        for (int row = 0; row < 3; row++)
        {
            result[row] = new string[3];
            for (int col = 0; col < 3; col++)
            {
                result[row][col] = board[row, col] == Player.X ? "X" :
                                   board[row, col] == Player.O ? "O" : "";
            }
        }
        return result;
    }
}
