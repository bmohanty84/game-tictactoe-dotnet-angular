using TicTacToe.Api.Models;

namespace TicTacToe.Api.Dtos;

public class CreateGameRequest
{
    public GameMode Mode { get; set; } = GameMode.TwoPlayer;
}

/// <summary>
/// The frontend may send either CellIndex (0-8) directly, or Row/Col (each 0-2).
/// If CellIndex is supplied it takes precedence; otherwise Row/Col are combined
/// into a cell index server-side. This mirrors the problem statement's wording
/// ("row and column, or cell index") without forcing the client to pick one shape.
/// </summary>
public class MoveRequest
{
    public Player Player { get; set; }
    public int? CellIndex { get; set; }
    public int? Row { get; set; }
    public int? Col { get; set; }
}
