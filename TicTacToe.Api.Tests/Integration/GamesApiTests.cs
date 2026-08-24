using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;
using Xunit;

namespace TicTacToe.Api.Tests.Integration;

public class GamesApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GamesApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateGame_ReturnsCreatedWithGameState()
    {
        // Arrange
        var request = new CreateGameRequest { GameMode = GameMode.TwoPlayer };

        // Act
        var response = await _client.PostAsJsonAsync("/api/games", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var gameState = await response.Content.ReadFromJsonAsync<GameStateResponse>();
        Assert.NotNull(gameState);
        Assert.Equal("InProgress", gameState.Status);
        Assert.Equal("TwoPlayer", gameState.GameMode);
        Assert.Equal("X", gameState.CurrentPlayer);
    }

    [Fact]
    public async Task GetGame_ExistingGame_ReturnsOk()
    {
        // Arrange - Create a game first
        var createResponse = await _client.PostAsJsonAsync(
            "/api/games", new CreateGameRequest { GameMode = GameMode.TwoPlayer });
        var gameState = await createResponse.Content.ReadFromJsonAsync<GameStateResponse>();

        // Act
        var response = await _client.GetAsync($"/api/games/{gameState!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var retrieved = await response.Content.ReadFromJsonAsync<GameStateResponse>();
        Assert.NotNull(retrieved);
        Assert.Equal(gameState.Id, retrieved.Id);
    }

    [Fact]
    public async Task GetGame_NonExistingGame_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/games/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MakeMove_ValidMove_ReturnsOkWithUpdatedState()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(
            "/api/games", new CreateGameRequest { GameMode = GameMode.TwoPlayer });
        var gameState = await createResponse.Content.ReadFromJsonAsync<GameStateResponse>();

        var moveRequest = new MakeMoveRequest
        {
            Player = Player.X,
            Row = 0,
            Column = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/games/{gameState!.Id}/moves", moveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedState = await response.Content.ReadFromJsonAsync<GameStateResponse>();
        Assert.NotNull(updatedState);
        Assert.Equal("O", updatedState.CurrentPlayer);
        Assert.Single(updatedState.MoveHistory);
    }

    [Fact]
    public async Task MakeMove_InvalidPosition_ReturnsBadRequest()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(
            "/api/games", new CreateGameRequest { GameMode = GameMode.TwoPlayer });
        var gameState = await createResponse.Content.ReadFromJsonAsync<GameStateResponse>();

        var moveRequest = new MakeMoveRequest
        {
            Player = Player.X,
            Row = 5,
            Column = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/games/{gameState!.Id}/moves", moveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UndoMove_ValidUndo_ReturnsOkWithUpdatedState()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(
            "/api/games", new CreateGameRequest { GameMode = GameMode.TwoPlayer });
        var gameState = await createResponse.Content.ReadFromJsonAsync<GameStateResponse>();

        // Make two moves
        await _client.PostAsJsonAsync($"/api/games/{gameState!.Id}/moves",
            new MakeMoveRequest { Player = Player.X, Row = 0, Column = 0 });
        await _client.PostAsJsonAsync($"/api/games/{gameState.Id}/moves",
            new MakeMoveRequest { Player = Player.O, Row = 1, Column = 1 });

        // Act
        var response = await _client.PostAsJsonAsync($"/api/games/{gameState.Id}/undo", new object());

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var undoneState = await response.Content.ReadFromJsonAsync<GameStateResponse>();
        Assert.NotNull(undoneState);
        Assert.Single(undoneState.MoveHistory);
    }

    [Fact]
    public async Task ResetGame_ExistingGame_ReturnsOkWithClearedState()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(
            "/api/games", new CreateGameRequest { GameMode = GameMode.TwoPlayer });
        var gameState = await createResponse.Content.ReadFromJsonAsync<GameStateResponse>();

        // Make a move
        await _client.PostAsJsonAsync($"/api/games/{gameState!.Id}/moves",
            new MakeMoveRequest { Player = Player.X, Row = 0, Column = 0 });

        // Act
        var response = await _client.PostAsJsonAsync($"/api/games/{gameState.Id}/reset", new object());

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resetState = await response.Content.ReadFromJsonAsync<GameStateResponse>();
        Assert.NotNull(resetState);
        Assert.Empty(resetState.MoveHistory);
        Assert.Equal("InProgress", resetState.Status);
    }
}
