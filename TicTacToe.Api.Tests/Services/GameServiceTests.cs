using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Tests.Services;

public class GameServiceTests
{
    private readonly GameService _gameService = new(new ComputerPlayerService());

    [Fact]
    public void CreateGame_ReturnsNewGameWithCorrectDefaults()
    {
        // Act
        var game = _gameService.CreateGame(GameMode.TwoPlayer);

        // Assert
        Assert.NotEqual(Guid.Empty, game.Id);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameMode.TwoPlayer, game.GameMode);
        Assert.Null(game.Winner);
        Assert.Empty(game.WinningCells);
        Assert.Empty(game.MoveHistory);
    }

    [Fact]
    public void GetGame_ExistingGame_ReturnsGame()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.TwoPlayer);

        // Act
        var result = _gameService.GetGame(game.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(game.Id, result.Id);
    }

    [Fact]
    public void GetGame_NonExistingGame_ReturnsNull()
    {
        // Act
        var result = _gameService.GetGame(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MakeMove_ValidMove_UpdatesBoard()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.TwoPlayer);

        // Act
        var (updatedGame, _) = _gameService.MakeMove(game.Id, Player.X, 0, 0);

        // Assert
        Assert.Equal(Player.X, updatedGame.Board[0, 0]);
        Assert.Single(updatedGame.MoveHistory);
        Assert.Equal(Player.O, updatedGame.CurrentPlayer);
    }

    [Fact]
    public void MakeMove_AlternatesTurns()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.TwoPlayer);

        // Act & Assert - X's turn
        var (gameAfterX, _) = _gameService.MakeMove(game.Id, Player.X, 0, 0);
        Assert.Equal(Player.O, gameAfterX.CurrentPlayer);

        // Act & Assert - O's turn
        var (gameAfterO, _) = _gameService.MakeMove(game.Id, Player.O, 1, 1);
        Assert.Equal(Player.X, gameAfterO.CurrentPlayer);
    }

    [Fact]
    public void MakeMove_RowWin_DetectsWinner()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.TwoPlayer);

        // X: (0,0), O: (1,0), X: (0,1), O: (1,1), X: (0,2) - X wins top row
        _gameService.MakeMove(game.Id, Player.X, 0, 0);
        _gameService.MakeMove(game.Id, Player.O, 1, 0);
        _gameService.MakeMove(game.Id, Player.X, 0, 1);
        _gameService.MakeMove(game.Id, Player.O, 1, 1);
        var (finalGame, scoreboardUpdated) = _gameService.MakeMove(game.Id, Player.X, 0, 2);

        // Assert
        Assert.Equal(GameStatus.Won, finalGame.Status);
        Assert.Equal(Player.X, finalGame.Winner);
        Assert.True(scoreboardUpdated);
        Assert.Equal(3, finalGame.WinningCells.Count);
    }

    [Fact]
    public void MakeMove_ColumnWin_DetectsWinner()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.TwoPlayer);

        // X: (0,0), O: (0,1), X: (1,0), O: (1,1), X: (2,0) - X wins left column
        _gameService.MakeMove(game.Id, Player.X, 0, 0);
        _gameService.MakeMove(game.Id, Player.O, 0, 1);
        _gameService.MakeMove(game.Id, Player.X, 1, 0);
        _gameService.MakeMove(game.Id, Player.O, 1, 1);
        var (finalGame, _) = _gameService.MakeMove(game.Id, Player.X, 2, 0);

        // Assert
        Assert.Equal(GameStatus.Won, finalGame.Status);
        Assert.Equal(Player.X, finalGame.Winner);
    }

    [Fact]
    public void MakeMove_DiagonalWin_DetectsWinner()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.TwoPlayer);

        // X: (0,0), O: (0,1), X: (1,1), O: (0,2), X: (2,2) - X wins diagonal
        _gameService.MakeMove(game.Id, Player.X, 0, 0);
        _gameService.MakeMove(game.Id, Player.O, 0, 1);
        _gameService.MakeMove(game.Id, Player.X, 1, 1);
        _gameService.MakeMove(game.Id, Player.O, 0, 2);
        var (finalGame, _) = _gameService.MakeMove(game.Id, Player.X, 2, 2);

        // Assert
        Assert.Equal(GameStatus.Won, finalGame.Status);
        Assert.Equal(Player.X, finalGame.Winner);
    }

    [Fact]
    public void MakeMove_Draw_DetectsDraw()
    {
        // Arrange - Create a draw scenario
        // X O X
        // X X O
        // O X O
        var game = _gameService.CreateGame(GameMode.TwoPlayer);

        // Moves: X(0,0), O(0,1), X(0,2), O(1,2), X(1,0), O(2,0), X(1,1), O(2,2), X(2,1)
        _gameService.MakeMove(game.Id, Player.X, 0, 0); // 1
        _gameService.MakeMove(game.Id, Player.O, 0, 1); // 2
        _gameService.MakeMove(game.Id, Player.X, 0, 2); // 3
        _gameService.MakeMove(game.Id, Player.O, 1, 2); // 4
        _gameService.MakeMove(game.Id, Player.X, 1, 0); // 5
        _gameService.MakeMove(game.Id, Player.O, 2, 0); // 6
        _gameService.MakeMove(game.Id, Player.X, 1, 1); // 7
        _gameService.MakeMove(game.Id, Player.O, 2, 2); // 8
        var (finalGame, scoreboardUpdated) = _gameService.MakeMove(game.Id, Player.X, 2, 1); // 9

        // Assert
        Assert.Equal(GameStatus.Draw, finalGame.Status);
        Assert.Null(finalGame.Winner);
        Assert.True(scoreboardUpdated);
        Assert.Equal(9, finalGame.MoveHistory.Count);
    }

    [Fact]
    public void MakeMove_AfterGameCompleted_ThrowsException()
    {
        // Arrange - Complete a game
        var game = _gameService.CreateGame(GameMode.TwoPlayer);
        _gameService.MakeMove(game.Id, Player.X, 0, 0);
        _gameService.MakeMove(game.Id, Player.O, 1, 0);
        _gameService.MakeMove(game.Id, Player.X, 0, 1);
        _gameService.MakeMove(game.Id, Player.O, 1, 1);
        _gameService.MakeMove(game.Id, Player.X, 0, 2); // X wins

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => _gameService.MakeMove(game.Id, Player.O, 2, 2));
        Assert.Contains("completed", ex.Message);
    }

    [Fact]
    public void MakeMove_WrongPlayer_ThrowsException()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.TwoPlayer);

        // Act & Assert - O tries to play first
        var ex = Assert.Throws<InvalidOperationException>(
            () => _gameService.MakeMove(game.Id, Player.O, 0, 0));
        Assert.Contains("turn", ex.Message);
    }

    [Fact]
    public void MakeMove_OccupiedCell_ThrowsException()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.TwoPlayer);
        _gameService.MakeMove(game.Id, Player.X, 0, 0);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => _gameService.MakeMove(game.Id, Player.O, 0, 0));
        Assert.Contains("occupied", ex.Message);
    }

    [Fact]
    public void MakeMove_InvalidPosition_ThrowsException()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.TwoPlayer);

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => _gameService.MakeMove(game.Id, Player.X, 5, 5));
    }

    [Fact]
    public void MakeMove_InvalidGameId_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => _gameService.MakeMove(Guid.NewGuid(), Player.X, 0, 0));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void UndoMove_TwoPlayerMode_RemovesLastMove()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.TwoPlayer);
        _gameService.MakeMove(game.Id, Player.X, 0, 0);
        _gameService.MakeMove(game.Id, Player.O, 1, 1);

        // Act
        var (undoneGame, _) = _gameService.UndoMove(game.Id);

        // Assert
        Assert.Single(undoneGame.MoveHistory);
        Assert.Equal(Player.O, undoneGame.CurrentPlayer); // O's turn restored
        Assert.Equal(GameStatus.InProgress, undoneGame.Status);
    }

    [Fact]
    public void UndoMove_CompletedGame_ThrowsException()
    {
        // Arrange - Complete a game
        var game = _gameService.CreateGame(GameMode.TwoPlayer);
        _gameService.MakeMove(game.Id, Player.X, 0, 0);
        _gameService.MakeMove(game.Id, Player.O, 1, 0);
        _gameService.MakeMove(game.Id, Player.X, 0, 1);
        _gameService.MakeMove(game.Id, Player.O, 1, 1);
        _gameService.MakeMove(game.Id, Player.X, 0, 2); // X wins

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => _gameService.UndoMove(game.Id));
        Assert.Contains("completed", ex.Message);
    }

    [Fact]
    public void UndoMove_NoMoves_ThrowsException()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.TwoPlayer);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => _gameService.UndoMove(game.Id));
        Assert.Contains("No moves", ex.Message);
    }

    [Fact]
    public void ResetGame_ClearsBoardAndState()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.TwoPlayer);
        _gameService.MakeMove(game.Id, Player.X, 0, 0);
        _gameService.MakeMove(game.Id, Player.O, 1, 1);

        // Act
        var resetGame = _gameService.ResetGame(game.Id);

        // Assert
        Assert.Equal(GameStatus.InProgress, resetGame.Status);
        Assert.Equal(Player.X, resetGame.CurrentPlayer);
        Assert.Null(resetGame.Winner);
        Assert.Empty(resetGame.MoveHistory);
        Assert.Empty(resetGame.WinningCells);
    }

    [Fact]
    public void ResetGame_InvalidId_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => _gameService.ResetGame(Guid.NewGuid()));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void MakeMove_OWins_DetectsCorrectly()
    {
        // Arrange - O wins
        var game = _gameService.CreateGame(GameMode.TwoPlayer);
        _gameService.MakeMove(game.Id, Player.X, 0, 0); // X
        _gameService.MakeMove(game.Id, Player.O, 1, 0); // O
        _gameService.MakeMove(game.Id, Player.X, 0, 1); // X
        _gameService.MakeMove(game.Id, Player.O, 1, 1); // O
        _gameService.MakeMove(game.Id, Player.X, 2, 2); // X
        var (finalGame, _) = _gameService.MakeMove(game.Id, Player.O, 1, 2); // O wins

        // Assert
        Assert.Equal(GameStatus.Won, finalGame.Status);
        Assert.Equal(Player.O, finalGame.Winner);
    }

    [Fact]
    public void MakeMove_KeepsMoveHistory()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.TwoPlayer);

        // Act
        _gameService.MakeMove(game.Id, Player.X, 0, 0);
        _gameService.MakeMove(game.Id, Player.O, 1, 1);

        // Assert
        var retrieved = _gameService.GetGame(game.Id)!;
        Assert.Equal(2, retrieved.MoveHistory.Count);
        Assert.Equal(Player.X, retrieved.MoveHistory[0].Player);
        Assert.Equal(0, retrieved.MoveHistory[0].Row);
        Assert.Equal(0, retrieved.MoveHistory[0].Column);
        Assert.Equal(Player.O, retrieved.MoveHistory[1].Player);
        Assert.Equal(1, retrieved.MoveHistory[1].Row);
        Assert.Equal(1, retrieved.MoveHistory[1].Column);
    }

    [Fact]
    public void UndoMove_ComputerMode_UndoesBothMoves()
    {
        // Arrange
        var game = _gameService.CreateGame(GameMode.Computer);
        // Player makes move; computer auto-plays in Computer mode
        _gameService.MakeMove(game.Id, Player.X, 0, 0);

        // Act
        var (undoneGame, _) = _gameService.UndoMove(game.Id);

        // Assert
        Assert.Empty(undoneGame.MoveHistory);
        Assert.Equal(Player.X, undoneGame.CurrentPlayer);
    }

    [Fact]
    public void MakeMove_SecondDiagonalWin()
    {
        // Arrange - X wins on anti-diagonal
        var game = _gameService.CreateGame(GameMode.TwoPlayer);
        _gameService.MakeMove(game.Id, Player.X, 0, 2);
        _gameService.MakeMove(game.Id, Player.O, 0, 0);
        _gameService.MakeMove(game.Id, Player.X, 1, 1);
        _gameService.MakeMove(game.Id, Player.O, 0, 1);
        var (finalGame, _) = _gameService.MakeMove(game.Id, Player.X, 2, 0);

        // Assert
        Assert.Equal(GameStatus.Won, finalGame.Status);
        Assert.Equal(Player.X, finalGame.Winner);
    }
}
