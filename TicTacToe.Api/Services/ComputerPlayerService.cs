using TicTacToe.Api.Interfaces;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Computer opponent with three difficulty levels.
/// Easy: mostly random. Medium: classic priority ladder (win &gt; block &gt; center &gt; corner &gt; any).
/// Hard: minimax with depth-aware scoring (never loses).
/// </summary>
public class ComputerPlayerService : IComputerPlayerService
{
    private readonly Random _random = new();

    private static readonly int[][] WinPatterns = new int[][]
    {
        // Rows
        new[] { 0, 1, 2 },
        new[] { 3, 4, 5 },
        new[] { 6, 7, 8 },
        // Columns
        new[] { 0, 3, 6 },
        new[] { 1, 4, 7 },
        new[] { 2, 5, 8 },
        // Diagonals
        new[] { 0, 4, 8 },
        new[] { 2, 4, 6 }
    };

    public Move GetBestMove(Player?[,] board, Player computerPlayer, Difficulty difficulty = Difficulty.Hard)
    {
        var humanPlayer = computerPlayer == Player.X ? Player.O : Player.X;

        // Easy: take an obvious win if it stumbles into one, otherwise play randomly
        if (difficulty == Difficulty.Easy)
        {
            var easyWin = FindWinningCell(board, computerPlayer);
            if (easyWin != null)
            {
                return new Move(computerPlayer, easyWin[0], easyWin[1], 0);
            }

            return GetRandomMove(board, computerPlayer);
        }

        // Medium: classic priority ladder —
        // 1. win  2. block  3. center  4. corner  5. any cell
        if (difficulty == Difficulty.Medium)
        {
            return GetMediumMove(board, computerPlayer, humanPlayer);
        }

        // 1. Take an immediate win if available
        var winningCell = FindWinningCell(board, computerPlayer);
        if (winningCell != null)
        {
            return new Move(computerPlayer, winningCell[0], winningCell[1], 0);
        }

        // 2. Block the human's immediate win
        var blockingCell = FindWinningCell(board, humanPlayer);
        if (blockingCell != null)
        {
            return new Move(computerPlayer, blockingCell[0], blockingCell[1], 0);
        }

        // 3. Fall back to full minimax search for optimal play
        int bestScore = int.MinValue;
        int[]? bestCell = null;

        foreach (var cell in GetEmptyCells(board))
        {
            board[cell[0], cell[1]] = computerPlayer;
            int score = Minimax(board, 0, isMaximizing: false, computerPlayer, humanPlayer)
                        + PositionBonus(cell);
            board[cell[0], cell[1]] = null;

            if (score > bestScore)
            {
                bestScore = score;
                bestCell = cell;
            }
        }

        if (bestCell == null)
        {
            throw new InvalidOperationException("No empty cells available.");
        }

        return new Move(computerPlayer, bestCell[0], bestCell[1], 0);
    }

    private Move GetRandomMove(Player?[,] board, Player computerPlayer)
    {
        var emptyCells = GetEmptyCells(board);
        if (emptyCells.Count == 0)
        {
            throw new InvalidOperationException("No empty cells available.");
        }

        var chosenCell = emptyCells[_random.Next(emptyCells.Count)];
        return new Move(computerPlayer, chosenCell[0], chosenCell[1], 0);
    }

    /// <summary>
    /// Medium difficulty: fixed priority ladder.
    /// 1. If O can win, play the winning move.
    /// 2. If X can win next, block X.
    /// 3. Take center if available.
    /// 4. Take a corner if available.
    /// 5. Take any available cell.
    /// </summary>
    private Move GetMediumMove(Player?[,] board, Player computerPlayer, Player humanPlayer)
    {
        // 1. Winning move for the computer
        var winningCell = FindWinningCell(board, computerPlayer);
        if (winningCell != null)
        {
            return new Move(computerPlayer, winningCell[0], winningCell[1], 0);
        }

        // 2. Block the human's winning move
        var blockingCell = FindWinningCell(board, humanPlayer);
        if (blockingCell != null)
        {
            return new Move(computerPlayer, blockingCell[0], blockingCell[1], 0);
        }

        // 3. Center
        if (board[1, 1] == null)
        {
            return new Move(computerPlayer, 1, 1, 0);
        }

        // 4. Corner
        int[][] corners =
        {
            new[] { 0, 0 }, new[] { 0, 2 },
            new[] { 2, 0 }, new[] { 2, 2 }
        };
        var corner = corners.FirstOrDefault(c => board[c[0], c[1]] == null);
        if (corner != null)
        {
            return new Move(computerPlayer, corner[0], corner[1], 0);
        }

        // 5. Any available cell
        return GetRandomMove(board, computerPlayer);
    }

    private static int[]? FindWinningCell(Player?[,] board, Player player)
    {
        foreach (var pattern in WinPatterns)
        {
            var cells = pattern.Select(i => new[] { i / 3, i % 3 }).ToList();
            int owned = cells.Count(c => board[c[0], c[1]] == player);
            int empty = cells.Count(c => board[c[0], c[1]] == null);

            if (owned == 2 && empty == 1)
            {
                return cells.First(c => board[c[0], c[1]] == null);
            }
        }

        return null;
    }

    private int Minimax(Player?[,] board, int depth, bool isMaximizing, Player computer, Player human)
    {
        var winner = GetWinner(board);
        if (winner == computer)
        {
            return 10 - depth; // prefer faster wins
        }

        if (winner == human)
        {
            return depth - 10; // prefer slower losses
        }

        if (!board.Cast<Player?>().Any(c => c == null))
        {
            return 0; // draw
        }

        if (isMaximizing)
        {
            int best = int.MinValue;
            foreach (var cell in GetEmptyCells(board))
            {
                board[cell[0], cell[1]] = computer;
                best = Math.Max(best, Minimax(board, depth + 1, false, computer, human));
                board[cell[0], cell[1]] = null;
            }

            return best;
        }
        else
        {
            int best = int.MaxValue;
            foreach (var cell in GetEmptyCells(board))
            {
                board[cell[0], cell[1]] = human;
                best = Math.Min(best, Minimax(board, depth + 1, true, computer, human));
                board[cell[0], cell[1]] = null;
            }

            return best;
        }
    }

    private static Player? GetWinner(Player?[,] board)
    {
        foreach (var pattern in WinPatterns)
        {
            var first = board[pattern[0] / 3, pattern[0] % 3];
            if (first != null
                && board[pattern[1] / 3, pattern[1] % 3] == first
                && board[pattern[2] / 3, pattern[2] % 3] == first)
            {
                return first;
            }
        }

        return null;
    }

    private static List<int[]> GetEmptyCells(Player?[,] board)
    {
        var emptyCells = new List<int[]>();
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (board[row, col] == null)
                {
                    emptyCells.Add(new[] { row, col });
                }
            }
        }

        return emptyCells;
    }

    /// <summary>
    /// Tie-breaker so equal-scoring moves favor center, then corners, then edges.
    /// Small enough to never override a win/loss/block decision.
    /// </summary>
    private static int PositionBonus(int[] cell)
    {
        if (cell[0] == 1 && cell[1] == 1) return 3;   // center
        if (cell[0] != 1 && cell[1] != 1) return 2;   // corners
        return 1;                                     // edges
    }
}
