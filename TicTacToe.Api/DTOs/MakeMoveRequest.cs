using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

public class MakeMoveRequest
{
    public Player Player { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
}
