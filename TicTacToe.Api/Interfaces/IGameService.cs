using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Interfaces;

public interface IGameService
{
    Game CreateGame(GameMode mode, Difficulty difficulty = Difficulty.Medium);
    Game? GetGame(Guid gameId);
    (Game game, bool scoreboardUpdated) MakeMove(Guid gameId, Player player, int row, int column);
    (Game game, bool scoreboardUpdated) UndoMove(Guid gameId);
    Game ResetGame(Guid gameId);
}
