namespace TicTacToe.Api.Models;

/// <summary>
/// One entry in a game's move history. CellIndex is the 0-8 position
/// (row * 3 + col); Row/Col are kept alongside it purely so the API/UI
/// don't have to re-derive them for display purposes.
/// </summary>
public class MoveRecord
{
    public int MoveNumber { get; init; }
    public Player Player { get; init; }
    public int Row { get; init; }
    public int Col { get; init; }
    public int CellIndex { get; init; }
    public bool IsComputerMove { get; init; }
}
