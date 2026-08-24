using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Interfaces;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreboardController : ControllerBase
{
    private readonly IScoreboardService _scoreboardService;

    public ScoreboardController(IScoreboardService scoreboardService)
    {
        _scoreboardService = scoreboardService;
    }

    /// <summary>
    /// Gets the current scoreboard.
    /// </summary>
    /// <returns>Scoreboard with X wins, O wins, and draws</returns>
    /// <response code="200">Scoreboard returned</response>
    [HttpGet]
    [ProducesResponseType(typeof(ScoreboardResponse), StatusCodes.Status200OK)]
    public IActionResult GetScoreboard()
    {
        var scoreboard = _scoreboardService.GetScoreboard();
        var response = ScoreboardResponse.FromScoreboard(scoreboard);
        return Ok(response);
    }

    /// <summary>
    /// Resets the scoreboard to zero.
    /// </summary>
    /// <returns>Reset scoreboard</returns>
    /// <response code="200">Scoreboard reset</response>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(ScoreboardResponse), StatusCodes.Status200OK)]
    public IActionResult ResetScoreboard()
    {
        _scoreboardService.Reset();
        var scoreboard = _scoreboardService.GetScoreboard();
        var response = ScoreboardResponse.FromScoreboard(scoreboard);
        return Ok(response);
    }
}
