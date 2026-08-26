using TicTacToe.Api.Models;

namespace TicTacToe.Api.Dtos;

public class MoveDto
{
    public int MoveNumber { get; set; }
    public Player Player { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public int CellIndex { get; set; }
    public bool IsComputerMove { get; set; }
}

/// <summary>
/// Everything the frontend needs to render the game in one payload -
/// see Clarification 1 in the problem statement (backend owns all game truth).
/// </summary>
public class GameStateResponse
{
    public Guid Id { get; set; }

    // Serialized as an array of 9 strings/nulls, e.g. ["X", null, "O", ...]
    public string?[] Board { get; set; } = new string?[9];

    public Player CurrentPlayer { get; set; }
    public GameMode Mode { get; set; }
    public GameStatus Status { get; set; }
    public Player? Winner { get; set; }
    public List<int>? WinningCells { get; set; }
    public List<MoveDto> Moves { get; set; } = new();

    // Convenience flag so the frontend doesn't have to re-derive undo eligibility.
    public bool CanUndo { get; set; }

    public static GameStateResponse FromSession(GameSession session)
    {
        return new GameStateResponse
        {
            Id = session.Id,
            Board = session.Board.Select(p => p?.ToString()).ToArray(),
            CurrentPlayer = session.CurrentPlayer,
            Mode = session.Mode,
            Status = session.Status,
            Winner = session.Winner,
            WinningCells = session.WinningCells,
            CanUndo = session.Status == GameStatus.InProgress && session.Moves.Count > 0,
            Moves = session.Moves.Select(m => new MoveDto
            {
                MoveNumber = m.MoveNumber,
                Player = m.Player,
                Row = m.Row,
                Col = m.Col,
                CellIndex = m.CellIndex,
                IsComputerMove = m.IsComputerMove
            }).ToList()
        };
    }
}

public class ScoreboardResponse
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }

    public static ScoreboardResponse FromModel(Scoreboard board) => new()
    {
        XWins = board.XWins,
        OWins = board.OWins,
        Draws = board.Draws
    };
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
}
