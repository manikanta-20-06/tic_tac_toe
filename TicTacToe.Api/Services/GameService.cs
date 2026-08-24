using System.Collections.Concurrent;
using TicTacToe.Api.Interfaces;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public class GameService : IGameService
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();
    private readonly IComputerPlayerService _computerPlayer;
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

    public GameService(IComputerPlayerService computerPlayer)
    {
        _computerPlayer = computerPlayer;
    }

    public Game CreateGame(GameMode mode, Difficulty difficulty = Difficulty.Medium)
    {
        var game = new Game(mode, difficulty);
        _games.TryAdd(game.Id, game);
        return game;
    }

    public Game? GetGame(Guid gameId)
    {
        _games.TryGetValue(gameId, out var game);
        return game;
    }

    public (Game game, bool scoreboardUpdated) MakeMove(Guid gameId, Player player, int row, int column)
    {
        var game = GetGame(gameId)
            ?? throw new InvalidOperationException("Game not found.");

        ValidateMove(game, player, row, column);

        game.Board[row, column] = player;
        var moveNumber = game.MoveHistory.Count + 1;
        var move = new Move(player, row, column, moveNumber);
        game.MoveHistory.Add(move);

        // Check for win
        var winResult = CheckForWin(game, player, row, column);
        if (winResult != null)
        {
            game.Status = GameStatus.Won;
            game.Winner = player;
            game.WinningCells = winResult;
            game.ScoreboardUpdated = true;
            return (game, true);
        }

        // Check for draw
        if (game.MoveHistory.Count == 9)
        {
            game.Status = GameStatus.Draw;
            game.ScoreboardUpdated = true;
            return (game, true);
        }

        // Alternate turns
        game.CurrentPlayer = player == Player.X ? Player.O : Player.X;

        // Auto-play computer move in Computer mode
        if (game.GameMode == GameMode.Computer && game.CurrentPlayer == Player.O && game.Status == GameStatus.InProgress)
        {
            var computerMove = _computerPlayer.GetBestMove(game.Board, Player.O, game.Difficulty);
            game.Board[computerMove.Row, computerMove.Column] = Player.O;
            var computerMoveNumber = game.MoveHistory.Count + 1;
            game.MoveHistory.Add(new Move(Player.O, computerMove.Row, computerMove.Column, computerMoveNumber));

            // Check if computer won
            var computerWinResult = CheckForWin(game, Player.O, computerMove.Row, computerMove.Column);
            if (computerWinResult != null)
            {
                game.Status = GameStatus.Won;
                game.Winner = Player.O;
                game.WinningCells = computerWinResult;
                game.ScoreboardUpdated = true;
                return (game, true);
            }

            // Check if computer caused a draw
            if (game.MoveHistory.Count == 9)
            {
                game.Status = GameStatus.Draw;
                game.ScoreboardUpdated = true;
                return (game, true);
            }

            // Switch back to player's turn
            game.CurrentPlayer = Player.X;
        }

        return (game, false);
    }

    public (Game game, bool scoreboardUpdated) UndoMove(Guid gameId)
    {
        var game = GetGame(gameId)
            ?? throw new InvalidOperationException("Game not found.");

        if (game.Status != GameStatus.InProgress)
        {
            throw new InvalidOperationException("Cannot undo after game is completed.");
        }

        if (game.MoveHistory.Count == 0)
        {
            throw new InvalidOperationException("No moves to undo.");
        }

        if (game.GameMode == GameMode.Computer)
        {
            return UndoComputerModeMoves(game);
        }
        else
        {
            return UndoTwoPlayerMove(game);
        }
    }

    private (Game game, bool scoreboardUpdated) UndoTwoPlayerMove(Game game)
    {
        var lastMove = game.MoveHistory[^1];
        game.Board[lastMove.Row, lastMove.Column] = null;
        game.MoveHistory.RemoveAt(game.MoveHistory.Count - 1);
        game.CurrentPlayer = lastMove.Player;
        return (game, false);
    }

    private (Game game, bool scoreboardUpdated) UndoComputerModeMoves(Game game)
    {
        if (game.MoveHistory.Count < 2)
        {
            return UndoTwoPlayerMove(game);
        }

        // Remove computer's move
        var computerMove = game.MoveHistory[^1];
        game.Board[computerMove.Row, computerMove.Column] = null;
        game.MoveHistory.RemoveAt(game.MoveHistory.Count - 1);

        // Remove player's move
        var playerMove = game.MoveHistory[^1];
        game.Board[playerMove.Row, playerMove.Column] = null;
        game.MoveHistory.RemoveAt(game.MoveHistory.Count - 1);

        game.CurrentPlayer = playerMove.Player;
        return (game, false);
    }

    public Game ResetGame(Guid gameId)
    {
        var game = GetGame(gameId)
            ?? throw new InvalidOperationException("Game not found.");

        game.Board = new Player?[3, 3];
        game.CurrentPlayer = Player.X;
        game.Status = GameStatus.InProgress;
        game.Winner = null;
        game.WinningCells = new List<int[]>();
        game.MoveHistory = new List<Move>();
        game.ScoreboardUpdated = false;

        return game;
    }

    private void ValidateMove(Game game, Player player, int row, int column)
    {
        if (game.Status != GameStatus.InProgress)
        {
            throw new InvalidOperationException("Game is already completed.");
        }

        if (game.CurrentPlayer != player)
        {
            throw new InvalidOperationException($"It's not {player}'s turn. Current player is {game.CurrentPlayer}.");
        }

        if (row < 0 || row > 2 || column < 0 || column > 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(row),
                $"Invalid position: ({row}, {column}). Row and column must be between 0 and 2.");
        }

        if (game.Board[row, column] != null)
        {
            throw new InvalidOperationException($"Cell ({row}, {column}) is already occupied.");
        }
    }

    private List<int[]>? CheckForWin(Game game, Player player, int lastRow, int lastColumn)
    {
        foreach (var pattern in WinPatterns)
        {
            var cells = pattern.Select(i => new[] { i / 3, i % 3 }).ToList();
            if (cells.Any(c => c[0] == lastRow && c[1] == lastColumn))
            {
                if (cells.All(c => game.Board[c[0], c[1]] == player))
                {
                    return cells;
                }
            }
        }

        return null;
    }
}
