using System.Collections.Concurrent;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Owns all game sessions in memory and implements the rules described in
/// the problem statement: move validation, win/draw detection, undo
/// (mode-aware), reset, and driving the computer opponent.
///
/// Chosen approach for Clarification 2 (Scoreboard vs Undo): Option A -
/// Undo is disabled once a game is Won or Draw. This keeps the scoreboard
/// simple and always consistent with completed-game state, at the cost of
/// not being able to "take back" a finished game. See README for rationale.
/// </summary>
public class GameService : IGameService
{
    private readonly ConcurrentDictionary<Guid, GameSession> _games = new();
    private readonly IScoreboardService _scoreboard;

    public GameService(IScoreboardService scoreboard)
    {
        _scoreboard = scoreboard;
    }

    public GameSession CreateGame(GameMode mode)
    {
        var session = new GameSession { Mode = mode };
        _games[session.Id] = session;
        return session;
    }

    public GameSession GetGame(Guid id)
    {
        if (!_games.TryGetValue(id, out var session))
        {
            throw new GameNotFoundException(id);
        }
        return session;
    }

    public GameSession MakeMove(Guid id, Player player, int cellIndex)
    {
        var session = GetGame(id);

        lock (session)
        {
            ValidateMove(session, player, cellIndex);

            ApplyMove(session, player, cellIndex, isComputerMove: false);
            EvaluateOutcome(session);

            // If the human's move didn't end the game and this is Computer
            // Mode, let the computer (always "O") respond immediately so the
            // API returns a state where it's the human's turn again.
            if (session.Mode == GameMode.VsComputer &&
                session.Status == GameStatus.InProgress &&
                session.CurrentPlayer == Player.O)
            {
                var computerCell = GameRules.SelectComputerMove(session.Board, Player.O);
                ApplyMove(session, Player.O, computerCell, isComputerMove: true);
                EvaluateOutcome(session);
            }

            return session;
        }
    }

    public GameSession Undo(Guid id)
    {
        var session = GetGame(id);

        lock (session)
        {
            if (session.Status != GameStatus.InProgress)
            {
                throw new UndoNotAllowedException("Undo is disabled once a game has been won or drawn.");
            }

            if (session.Moves.Count == 0)
            {
                throw new UndoNotAllowedException("There are no moves to undo.");
            }

            // Two Player Mode: remove only the single most recent move.
            // Computer Mode: remove the computer's last move together with
            // the human move that preceded it, so the human always ends up
            // back at their own turn (see problem statement's worked example).
            var movesToRemove = session.Mode == GameMode.VsComputer ? 2 : 1;
            movesToRemove = Math.Min(movesToRemove, session.Moves.Count);

            for (var i = 0; i < movesToRemove; i++)
            {
                var lastMove = session.Moves[^1];
                session.Moves.RemoveAt(session.Moves.Count - 1);
                session.Board[lastMove.CellIndex] = null;
                // Whoever made the move that was just undone gets to move again.
                session.CurrentPlayer = lastMove.Player;
            }

            // Defensive recalculation - with Option A this should always land
            // back on InProgress, since undo is blocked on completed games,
            // but recomputing from the board keeps this correct if that
            // policy ever changes.
            RecalculateStatusFromBoard(session);

            return session;
        }
    }

    public GameSession ResetGame(Guid id)
    {
        var existing = GetGame(id);

        lock (existing)
        {
            // Reset Game starts a fresh session but intentionally keeps the
            // same Id and Mode, and never touches the scoreboard.
            var fresh = new GameSession { Id = existing.Id, Mode = existing.Mode };

            _games[existing.Id] = fresh;
            return fresh;
        }
    }

    // ----- internal helpers -----

    private static void ValidateMove(GameSession session, Player player, int cellIndex)
    {
        if (session.Status != GameStatus.InProgress)
        {
            throw new InvalidMoveException("This game has already finished.");
        }

        if (cellIndex < 0 || cellIndex > 8)
        {
            throw new InvalidMoveException("Cell index must be between 0 and 8.");
        }

        if (player != session.CurrentPlayer)
        {
            throw new InvalidMoveException($"It is not {player}'s turn.");
        }

        if (session.Board[cellIndex] is not null)
        {
            throw new InvalidMoveException("That cell is already occupied.");
        }

        if (session.Mode == GameMode.VsComputer && player == Player.O)
        {
            // In Computer Mode, O is always driven by the server, never a
            // direct client request.
            throw new InvalidMoveException("O is controlled by the computer in this mode.");
        }
    }

    private static void ApplyMove(GameSession session, Player player, int cellIndex, bool isComputerMove)
    {
        session.Board[cellIndex] = player;
        session.Moves.Add(new MoveRecord
        {
            MoveNumber = session.Moves.Count + 1,
            Player = player,
            Row = cellIndex / 3,
            Col = cellIndex % 3,
            CellIndex = cellIndex,
            IsComputerMove = isComputerMove
        });
        session.CurrentPlayer = player.Other();
    }

    private void EvaluateOutcome(GameSession session)
    {
        var winningLine = GameRules.FindWinningLine(session.Board);
        if (winningLine is not null)
        {
            session.Status = GameStatus.Won;
            session.Winner = session.Board[winningLine[0]];
            session.WinningCells = winningLine.ToList();
            RecordScoreboardOnce(session);
            return;
        }

        if (GameRules.IsBoardFull(session.Board))
        {
            session.Status = GameStatus.Draw;
            session.Winner = null;
            session.WinningCells = null;
            RecordScoreboardOnce(session);
        }
    }

    private void RecordScoreboardOnce(GameSession session)
    {
        // Guarantees the scoreboard updates exactly once per completed game,
        // even if outcome evaluation were ever triggered more than once.
        if (session.ScoreboardUpdated) return;

        if (session.Status == GameStatus.Won && session.Winner is not null)
        {
            _scoreboard.RecordWin(session.Winner.Value);
        }
        else if (session.Status == GameStatus.Draw)
        {
            _scoreboard.RecordDraw();
        }
        session.ScoreboardUpdated = true;
    }

    private static void RecalculateStatusFromBoard(GameSession session)
    {
        var winningLine = GameRules.FindWinningLine(session.Board);
        if (winningLine is not null)
        {
            session.Status = GameStatus.Won;
            session.Winner = session.Board[winningLine[0]];
            session.WinningCells = winningLine.ToList();
            return;
        }

        if (GameRules.IsBoardFull(session.Board))
        {
            session.Status = GameStatus.Draw;
            session.Winner = null;
            session.WinningCells = null;
            return;
        }

        session.Status = GameStatus.InProgress;
        session.Winner = null;
        session.WinningCells = null;
    }
}
