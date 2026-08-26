using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Dtos;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/scoreboard")]
public class ScoreboardController : ControllerBase
{
    private readonly IScoreboardService _scoreboardService;

    public ScoreboardController(IScoreboardService scoreboardService)
    {
        _scoreboardService = scoreboardService;
    }

    // GET /api/scoreboard
    [HttpGet]
    public ActionResult<ScoreboardResponse> Get()
    {
        return Ok(ScoreboardResponse.FromModel(_scoreboardService.GetScoreboard()));
    }

    // POST /api/scoreboard/reset
    [HttpPost("reset")]
    public ActionResult<ScoreboardResponse> Reset()
    {
        _scoreboardService.Reset();
        return Ok(ScoreboardResponse.FromModel(_scoreboardService.GetScoreboard()));
    }
}
