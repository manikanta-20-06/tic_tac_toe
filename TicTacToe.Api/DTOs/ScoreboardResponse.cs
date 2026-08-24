using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

public class ScoreboardResponse
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }

    public static ScoreboardResponse FromScoreboard(Scoreboard scoreboard)
    {
        return new ScoreboardResponse
        {
            XWins = scoreboard.XWins,
            OWins = scoreboard.OWins,
            Draws = scoreboard.Draws
        };
    }
}
