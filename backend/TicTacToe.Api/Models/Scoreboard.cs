namespace TicTacToe.Api.Models;

/// <summary>
/// A single, process-wide scoreboard shared across all game sessions,
/// matching the requirement that the scoreboard is "session-level" (i.e.
/// persists across individual games) and is only ever reset explicitly.
/// </summary>
public class Scoreboard
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }
}
