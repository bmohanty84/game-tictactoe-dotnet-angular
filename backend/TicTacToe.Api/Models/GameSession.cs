namespace TicTacToe.Api.Models;

/// <summary>
/// The full server-side state of a single Tic Tac Toe game.
/// The backend is the single source of truth for all of this -
/// the frontend only ever renders what it's given back from the API.
/// </summary>
public class GameSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // 9 cells, index = row * 3 + col. null = empty.
    public Player?[] Board { get; init; } = new Player?[9];

    public Player CurrentPlayer { get; set; } = Player.X;

    public GameMode Mode { get; init; }

    public GameStatus Status { get; set; } = GameStatus.InProgress;

    public Player? Winner { get; set; }

    // The 3 cell indices that make up the winning line, for UI highlighting.
    public List<int>? WinningCells { get; set; }

    public List<MoveRecord> Moves { get; init; } = new();

    /// <summary>
    /// True once this game's outcome (win/draw) has already been recorded on
    /// the scoreboard. Guarantees the scoreboard is only ever updated once
    /// per completed game, even if state is re-fetched or re-evaluated.
    /// </summary>
    public bool ScoreboardUpdated { get; set; }
}
