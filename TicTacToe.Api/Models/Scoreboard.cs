namespace TicTacToe.Api.Models;

public class Scoreboard
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }

    public Scoreboard()
    {
        XWins = 0;
        OWins = 0;
        Draws = 0;
    }

    public void Reset()
    {
        XWins = 0;
        OWins = 0;
        Draws = 0;
    }
}
