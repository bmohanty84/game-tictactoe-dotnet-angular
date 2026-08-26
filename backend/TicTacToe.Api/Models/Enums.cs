namespace TicTacToe.Api.Models;

public enum GameMode
{
    TwoPlayer,
    VsComputer
}

public enum GameStatus
{
    InProgress,
    Won,
    Draw
}

// "Player" is intentionally a simple enum with two values (X, O) rather than
// a free-form string, so the model layer cannot represent an invalid player.
public enum Player
{
    X,
    O
}

public static class PlayerExtensions
{
    public static Player Other(this Player player) => player == Player.X ? Player.O : Player.X;
}
