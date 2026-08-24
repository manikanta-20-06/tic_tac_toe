using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Interfaces;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IScoreboardService _scoreboardService;

    public GamesController(IGameService gameService, IScoreboardService scoreboardService)
    {
        _gameService = gameService;
        _scoreboardService = scoreboardService;
    }

    /// <summary>
    /// Creates a new game.
    /// </summary>
    /// <param name="request">Game configuration</param>
    /// <returns>New game state</returns>
    /// <response code="201">Game created successfully</response>
    [HttpPost]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status201Created)]
    public IActionResult CreateGame([FromBody] CreateGameRequest request)
    {
        var game = _gameService.CreateGame(request.GameMode, request.Difficulty);
        var response = GameStateResponse.FromGame(game);
        return CreatedAtAction(nameof(GetGame), new { id = game.Id }, response);
    }

    /// <summary>
    /// Gets the current state of a game.
    /// </summary>
    /// <param name="id">Game ID</param>
    /// <returns>Current game state</returns>
    /// <response code="200">Game state returned</response>
    /// <response code="404">Game not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetGame(Guid id)
    {
        var game = _gameService.GetGame(id);
        if (game == null)
        {
            return NotFound(new ErrorResponse($"Game with ID '{id}' not found."));
        }

        var response = GameStateResponse.FromGame(game);
        return Ok(response);
    }

    /// <summary>
    /// Makes a move in the game.
    /// </summary>
    /// <param name="id">Game ID</param>
    /// <param name="request">Move details (player, row, column)</param>
    /// <returns>Updated game state</returns>
    /// <response code="200">Move made successfully</response>
    /// <response code="400">Invalid move</response>
    /// <response code="404">Game not found</response>
    [HttpPost("{id:guid}/moves")]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult MakeMove(Guid id, [FromBody] MakeMoveRequest request)
    {
        try
        {
            var (game, scoreboardUpdated) = _gameService.MakeMove(id, request.Player, request.Row, request.Column);

            if (scoreboardUpdated && game.Winner.HasValue)
            {
                _scoreboardService.RecordResult(game.Status, game.Winner);
            }
            else if (scoreboardUpdated && game.Status == GameStatus.Draw)
            {
                _scoreboardService.RecordResult(game.Status, null);
            }

            var response = GameStateResponse.FromGame(game);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
            return BadRequest(new ErrorResponse(ex.Message));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Undoes the last move.
    /// </summary>
    /// <param name="id">Game ID</param>
    /// <returns>Updated game state after undo</returns>
    /// <response code="200">Undo successful</response>
    /// <response code="400">Cannot undo</response>
    /// <response code="404">Game not found</response>
    [HttpPost("{id:guid}/undo")]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult UndoMove(Guid id)
    {
        try
        {
            var (game, _) = _gameService.UndoMove(id);
            var response = GameStateResponse.FromGame(game);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Resets the game board while keeping the same game ID.
    /// </summary>
    /// <param name="id">Game ID</param>
    /// <returns>Reset game state</returns>
    /// <response code="200">Game reset successfully</response>
    /// <response code="404">Game not found</response>
    [HttpPost("{id:guid}/reset")]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult ResetGame(Guid id)
    {
        try
        {
            var game = _gameService.ResetGame(id);
            var response = GameStateResponse.FromGame(game);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
    }
}
