using TicTacToe.Api.Models;

namespace TicTacToe.Api.Interfaces;

public interface IScoreboardService
{
    Scoreboard GetScoreboard();
    void RecordResult(GameStatus status, Player? winner);
    void Reset();
}
