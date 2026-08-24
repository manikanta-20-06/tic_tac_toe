using TicTacToe.Api.Models;

namespace TicTacToe.Api.Interfaces;

public interface IComputerPlayerService
{
    /// <summary>
    /// Calculates the best move for the computer player at the given difficulty.
    /// Defaults to Hard (perfect minimax) when not specified.
    /// </summary>
    Move GetBestMove(Player?[,] board, Player computerPlayer, Difficulty difficulty = Difficulty.Hard);
}
