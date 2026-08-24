using TicTacToe.Api.Interfaces;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public class ScoreboardService : IScoreboardService
{
    private readonly Scoreboard _scoreboard = new();

    public Scoreboard GetScoreboard()
    {
        return _scoreboard;
    }

    public void RecordResult(GameStatus status, Player? winner)
    {
        switch (status)
        {
            case GameStatus.Won when winner == Player.X:
                _scoreboard.XWins++;
                break;
            case GameStatus.Won when winner == Player.O:
                _scoreboard.OWins++;
                break;
            case GameStatus.Draw:
                _scoreboard.Draws++;
                break;
        }
    }

    public void Reset()
    {
        _scoreboard.Reset();
    }
}
