using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;
using Xunit;

namespace TicTacToe.Api.Tests.Integration;

public class ScoreboardApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ScoreboardApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetScoreboard_ReturnsOkWithScoreboard()
    {
        // Reset scoreboard first (singleton may have state from other tests)
        await _client.PostAsJsonAsync("/api/scoreboard/reset", new object());

        // Act
        var response = await _client.GetAsync("/api/scoreboard");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var scoreboard = await response.Content.ReadFromJsonAsync<ScoreboardResponse>();
        Assert.NotNull(scoreboard);
        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }

    [Fact]
    public async Task ResetScoreboard_ReturnsOkWithResetScoreboard()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/scoreboard/reset", new object());

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var scoreboard = await response.Content.ReadFromJsonAsync<ScoreboardResponse>();
        Assert.NotNull(scoreboard);
        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }

    [Fact]
    public async Task Scoreboard_UpdatesAfterWin()
    {
        // Arrange - Create and complete a game
        var createResponse = await _client.PostAsJsonAsync(
            "/api/games", new CreateGameRequest { GameMode = GameMode.TwoPlayer });
        var gameState = await createResponse.Content.ReadFromJsonAsync<GameStateResponse>();

        // X wins: X(0,0), O(1,0), X(0,1), O(1,1), X(0,2)
        await _client.PostAsJsonAsync($"/api/games/{gameState!.Id}/moves",
            new MakeMoveRequest { Player = Player.X, Row = 0, Column = 0 });
        await _client.PostAsJsonAsync($"/api/games/{gameState.Id}/moves",
            new MakeMoveRequest { Player = Player.O, Row = 1, Column = 0 });
        await _client.PostAsJsonAsync($"/api/games/{gameState.Id}/moves",
            new MakeMoveRequest { Player = Player.X, Row = 0, Column = 1 });
        await _client.PostAsJsonAsync($"/api/games/{gameState.Id}/moves",
            new MakeMoveRequest { Player = Player.O, Row = 1, Column = 1 });
        await _client.PostAsJsonAsync($"/api/games/{gameState.Id}/moves",
            new MakeMoveRequest { Player = Player.X, Row = 0, Column = 2 });

        // Act
        var scoreboardResponse = await _client.GetAsync("/api/scoreboard");

        // Assert
        var scoreboard = await scoreboardResponse.Content.ReadFromJsonAsync<ScoreboardResponse>();
        Assert.NotNull(scoreboard);
        Assert.Equal(1, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }
}
