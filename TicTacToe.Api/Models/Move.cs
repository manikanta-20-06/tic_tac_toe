namespace TicTacToe.Api.Models;

public class Move
{
    public Player Player { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public int MoveNumber { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Move() { }

    public Move(Player player, int row, int column, int moveNumber)
    {
        Player = player;
        Row = row;
        Column = column;
        MoveNumber = moveNumber;
        Timestamp = DateTime.UtcNow;
    }
}
