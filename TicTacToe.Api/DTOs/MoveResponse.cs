using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

public class MoveResponse
{
    public string Player { get; set; } = string.Empty;
    public int Row { get; set; }
    public int Column { get; set; }
    public int MoveNumber { get; set; }
    public DateTime Timestamp { get; set; }

    public static MoveResponse FromMove(Move move)
    {
        return new MoveResponse
        {
            Player = move.Player.ToString(),
            Row = move.Row,
            Column = move.Column,
            MoveNumber = move.MoveNumber,
            Timestamp = move.Timestamp
        };
    }
}
