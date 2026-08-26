using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    GameSession CreateGame(GameMode mode);

    GameSession GetGame(Guid id);

    /// <param name="player">Which player the move is being submitted for.</param>
    /// <param name="cellIndex">0-8 target cell.</param>
    GameSession MakeMove(Guid id, Player player, int cellIndex);

    GameSession Undo(Guid id);

    GameSession ResetGame(Guid id);
}
