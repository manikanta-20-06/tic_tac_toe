using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Tests.Services;

public class ScoreboardServiceTests
{
    private readonly ScoreboardService _scoreboardService = new();

    [Fact]
    public void GetScoreboard_InitialScoreboard_ReturnsZeros()
    {
        // Act
        var scoreboard = _scoreboardService.GetScoreboard();

        // Assert
        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }

    [Fact]
    public void RecordResult_XWin_IncrementsXWins()
    {
        // Act
        _scoreboardService.RecordResult(GameStatus.Won, Player.X);
        var scoreboard = _scoreboardService.GetScoreboard();

        // Assert
        Assert.Equal(1, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }

    [Fact]
    public void RecordResult_OWin_IncrementsOWins()
    {
        // Act
        _scoreboardService.RecordResult(GameStatus.Won, Player.O);
        var scoreboard = _scoreboardService.GetScoreboard();

        // Assert
        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(1, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }

    [Fact]
    public void RecordResult_Draw_IncrementsDraws()
    {
        // Act
        _scoreboardService.RecordResult(GameStatus.Draw, null);
        var scoreboard = _scoreboardService.GetScoreboard();

        // Assert
        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(1, scoreboard.Draws);
    }

    [Fact]
    public void Reset_ClearsAllScores()
    {
        // Arrange
        _scoreboardService.RecordResult(GameStatus.Won, Player.X);
        _scoreboardService.RecordResult(GameStatus.Won, Player.O);
        _scoreboardService.RecordResult(GameStatus.Draw, null);

        // Act
        _scoreboardService.Reset();
        var scoreboard = _scoreboardService.GetScoreboard();

        // Assert
        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }

    [Fact]
    public void RecordResult_MultipleXWins_AccumulatesCorrectly()
    {
        // Act
        _scoreboardService.RecordResult(GameStatus.Won, Player.X);
        _scoreboardService.RecordResult(GameStatus.Won, Player.X);
        _scoreboardService.RecordResult(GameStatus.Won, Player.X);
        var scoreboard = _scoreboardService.GetScoreboard();

        // Assert
        Assert.Equal(3, scoreboard.XWins);
    }
}
