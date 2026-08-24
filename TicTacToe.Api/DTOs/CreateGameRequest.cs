using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

public class CreateGameRequest
{
    public GameMode GameMode { get; set; } = GameMode.TwoPlayer;
    public Difficulty Difficulty { get; set; } = Difficulty.Medium;
}
