namespace TicTacToe.Api.Services;

public class GameNotFoundException : Exception
{
    public GameNotFoundException(Guid id) : base($"No game found with id '{id}'.") { }
}

/// <summary>
/// Thrown for any rejected move: out-of-range cell, occupied cell,
/// wrong player's turn, or move submitted after the game already ended.
/// </summary>
public class InvalidMoveException : Exception
{
    public InvalidMoveException(string message) : base(message) { }
}

public class UndoNotAllowedException : Exception
{
    public UndoNotAllowedException(string message) : base(message) { }
}
