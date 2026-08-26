using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Dtos;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    // POST /api/games
    [HttpPost]
    public ActionResult<GameStateResponse> CreateGame([FromBody] CreateGameRequest request)
    {
        var session = _gameService.CreateGame(request.Mode);
        var response = GameStateResponse.FromSession(session);
        return CreatedAtAction(nameof(GetGame), new { id = session.Id }, response);
    }

    // GET /api/games/{id}
    [HttpGet("{id:guid}")]
    public ActionResult<GameStateResponse> GetGame(Guid id)
    {
        try
        {
            var session = _gameService.GetGame(id);
            return Ok(GameStateResponse.FromSession(session));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    // POST /api/games/{id}/moves
    [HttpPost("{id:guid}/moves")]
    public ActionResult<GameStateResponse> MakeMove(Guid id, [FromBody] MoveRequest request)
    {
        try
        {
            var cellIndex = request.CellIndex ?? ToCellIndex(request.Row, request.Col);
            var session = _gameService.MakeMove(id, request.Player, cellIndex);
            return Ok(GameStateResponse.FromSession(session));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (InvalidMoveException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
    }

    // POST /api/games/{id}/undo
    [HttpPost("{id:guid}/undo")]
    public ActionResult<GameStateResponse> Undo(Guid id)
    {
        try
        {
            var session = _gameService.Undo(id);
            return Ok(GameStateResponse.FromSession(session));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (UndoNotAllowedException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
    }

    // POST /api/games/{id}/reset
    [HttpPost("{id:guid}/reset")]
    public ActionResult<GameStateResponse> ResetGame(Guid id)
    {
        try
        {
            var session = _gameService.ResetGame(id);
            return Ok(GameStateResponse.FromSession(session));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    private static int ToCellIndex(int? row, int? col)
    {
        if (row is null || col is null)
        {
            throw new InvalidMoveException("Either CellIndex, or both Row and Col, must be provided.");
        }
        if (row < 0 || row > 2 || col < 0 || col > 2)
        {
            throw new InvalidMoveException("Row and Col must each be between 0 and 2.");
        }
        return row.Value * 3 + col.Value;
    }
}
