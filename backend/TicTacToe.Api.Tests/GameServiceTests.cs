using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Api.Tests;

public class GameServiceTests
{
    private static GameService NewService(out IScoreboardService scoreboard)
    {
        scoreboard = new ScoreboardService();
        return new GameService(scoreboard);
    }

    // ---------- Valid move ----------

    [Fact]
    public void MakeMove_ValidMove_PlacesMarkAndSwitchesTurn()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        var updated = service.MakeMove(game.Id, Player.X, 0);

        Assert.Equal(Player.X, updated.Board[0]);
        Assert.Equal(Player.O, updated.CurrentPlayer);
        Assert.Single(updated.Moves);
        Assert.Equal(GameStatus.InProgress, updated.Status);
    }

    // ---------- Invalid move ----------

    [Theory]
    [InlineData(-1)]
    [InlineData(9)]
    public void MakeMove_CellOutOfRange_Throws(int cellIndex)
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        Assert.Throws<InvalidMoveException>(() => service.MakeMove(game.Id, Player.X, cellIndex));
    }

    [Fact]
    public void MakeMove_OccupiedCell_Throws()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(game.Id, Player.X, 0);

        Assert.Throws<InvalidMoveException>(() => service.MakeMove(game.Id, Player.O, 0));
    }

    [Fact]
    public void MakeMove_WrongPlayersTurn_DoesNotChangeCurrentPlayer()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        // It's X's turn; O tries to move.
        Assert.Throws<InvalidMoveException>(() => service.MakeMove(game.Id, Player.O, 4));

        var current = service.GetGame(game.Id);
        Assert.Equal(Player.X, current.CurrentPlayer);
        Assert.Empty(current.Moves);
    }

    // ---------- Turn switching ----------

    [Fact]
    public void MakeMove_AlternatesTurnsAcrossMultipleMoves()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        var afterX = service.MakeMove(game.Id, Player.X, 0);
        Assert.Equal(Player.O, afterX.CurrentPlayer);

        var afterO = service.MakeMove(game.Id, Player.O, 1);
        Assert.Equal(Player.X, afterO.CurrentPlayer);
    }

    // ---------- Win detection: row / column / diagonal ----------

    [Fact]
    public void MakeMove_CompletingRow_DeclaresWinnerAndHighlightsLine()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        // X: 0,1,2 (top row) | O: 3,4
        service.MakeMove(game.Id, Player.X, 0);
        service.MakeMove(game.Id, Player.O, 3);
        service.MakeMove(game.Id, Player.X, 1);
        service.MakeMove(game.Id, Player.O, 4);
        var result = service.MakeMove(game.Id, Player.X, 2);

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new List<int> { 0, 1, 2 }, result.WinningCells);
    }

    [Fact]
    public void MakeMove_CompletingColumn_DeclaresWinner()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        // X: 0,3,6 (left column) | O: 1,2
        service.MakeMove(game.Id, Player.X, 0);
        service.MakeMove(game.Id, Player.O, 1);
        service.MakeMove(game.Id, Player.X, 3);
        service.MakeMove(game.Id, Player.O, 2);
        var result = service.MakeMove(game.Id, Player.X, 6);

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new List<int> { 0, 3, 6 }, result.WinningCells);
    }

    [Fact]
    public void MakeMove_CompletingDiagonal_DeclaresWinner()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        // X: 0,4,8 (diagonal) | O: 1,2
        service.MakeMove(game.Id, Player.X, 0);
        service.MakeMove(game.Id, Player.O, 1);
        service.MakeMove(game.Id, Player.X, 4);
        service.MakeMove(game.Id, Player.O, 2);
        var result = service.MakeMove(game.Id, Player.X, 8);

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new List<int> { 0, 4, 8 }, result.WinningCells);
    }

    // ---------- Draw ----------

    [Fact]
    public void MakeMove_BoardFullNoWinner_IsDraw()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        // X O X
        // X O O
        // O X X
        var sequence = new[]
        {
            (Player.X, 0), (Player.O, 1), (Player.X, 2),
            (Player.O, 4), (Player.X, 3), (Player.O, 5),
            (Player.X, 7), (Player.O, 6), (Player.X, 8),
        };

        GameSession? last = null;
        foreach (var (player, cell) in sequence)
        {
            last = service.MakeMove(game.Id, player, cell);
        }

        Assert.Equal(GameStatus.Draw, last!.Status);
        Assert.Null(last.Winner);
    }

    // ---------- Move after completion ----------

    [Fact]
    public void MakeMove_AfterGameWon_Throws()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, Player.X, 0);
        service.MakeMove(game.Id, Player.O, 3);
        service.MakeMove(game.Id, Player.X, 1);
        service.MakeMove(game.Id, Player.O, 4);
        service.MakeMove(game.Id, Player.X, 2); // X wins

        Assert.Throws<InvalidMoveException>(() => service.MakeMove(game.Id, Player.O, 5));
    }

    // ---------- Reset ----------

    [Fact]
    public void ResetGame_ClearsBoardHistoryAndStatus_ButKeepsScoreboard()
    {
        var service = NewService(out var scoreboard);
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, Player.X, 0);
        service.MakeMove(game.Id, Player.O, 3);
        service.MakeMove(game.Id, Player.X, 1);
        service.MakeMove(game.Id, Player.O, 4);
        service.MakeMove(game.Id, Player.X, 2); // X wins -> scoreboard updated

        var reset = service.ResetGame(game.Id);

        Assert.All(reset.Board, cell => Assert.Null(cell));
        Assert.Empty(reset.Moves);
        Assert.Equal(GameStatus.InProgress, reset.Status);
        Assert.Equal(Player.X, reset.CurrentPlayer);
        Assert.Null(reset.Winner);

        // Scoreboard from the completed game before reset must be preserved.
        Assert.Equal(1, scoreboard.GetScoreboard().XWins);
    }

    // ---------- Undo: two-player mode ----------

    [Fact]
    public void Undo_TwoPlayerMode_RemovesOnlyLastMove()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, Player.X, 0);
        service.MakeMove(game.Id, Player.O, 4);

        var afterUndo = service.Undo(game.Id);

        Assert.Single(afterUndo.Moves);
        Assert.Equal(Player.X, afterUndo.Board[0]);
        Assert.Null(afterUndo.Board[4]);
        Assert.Equal(Player.O, afterUndo.CurrentPlayer); // O's move was undone -> O's turn again
    }

    [Fact]
    public void Undo_NoMovesYet_Throws()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        Assert.Throws<UndoNotAllowedException>(() => service.Undo(game.Id));
    }

    [Fact]
    public void Undo_AfterGameCompleted_IsDisabled()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, Player.X, 0);
        service.MakeMove(game.Id, Player.O, 3);
        service.MakeMove(game.Id, Player.X, 1);
        service.MakeMove(game.Id, Player.O, 4);
        service.MakeMove(game.Id, Player.X, 2); // X wins

        Assert.Throws<UndoNotAllowedException>(() => service.Undo(game.Id));
    }

    // ---------- Undo: computer mode ----------

    [Fact]
    public void Undo_ComputerMode_RemovesComputerMoveAndPrecedingHumanMoveTogether()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.VsComputer);

        // Human (X) plays; computer (O) auto-responds inside MakeMove.
        var afterHumanMove = service.MakeMove(game.Id, Player.X, 0);
        Assert.Equal(2, afterHumanMove.Moves.Count); // X's move + computer's reply

        var afterUndo = service.Undo(game.Id);

        Assert.Empty(afterUndo.Moves);
        Assert.Null(afterUndo.Board[0]);
        Assert.Equal(Player.X, afterUndo.CurrentPlayer); // back to human's turn
    }

    // ---------- Scoreboard ----------

    [Fact]
    public void Scoreboard_UpdatesExactlyOncePerCompletedGame()
    {
        var service = NewService(out var scoreboard);
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, Player.X, 0);
        service.MakeMove(game.Id, Player.O, 3);
        service.MakeMove(game.Id, Player.X, 1);
        service.MakeMove(game.Id, Player.O, 4);
        service.MakeMove(game.Id, Player.X, 2); // X wins

        // Re-fetching the (already completed) game must not double count.
        service.GetGame(game.Id);

        Assert.Equal(1, scoreboard.GetScoreboard().XWins);
        Assert.Equal(0, scoreboard.GetScoreboard().OWins);
        Assert.Equal(0, scoreboard.GetScoreboard().Draws);
    }

    [Fact]
    public void Scoreboard_ResetScoreboard_ClearsAllCounts()
    {
        var service = NewService(out var scoreboard);
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, Player.X, 0);
        service.MakeMove(game.Id, Player.O, 3);
        service.MakeMove(game.Id, Player.X, 1);
        service.MakeMove(game.Id, Player.O, 4);
        service.MakeMove(game.Id, Player.X, 2); // X wins

        scoreboard.Reset();

        var result = scoreboard.GetScoreboard();
        Assert.Equal(0, result.XWins);
        Assert.Equal(0, result.OWins);
        Assert.Equal(0, result.Draws);
    }

    // ---------- Computer move selection (integration through GameService) ----------

    [Fact]
    public void ComputerMode_ComputerBlocksImminentHumanWin()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.VsComputer);

        // X: 0, computer responds (likely center per priority rules).
        var afterFirst = service.MakeMove(game.Id, Player.X, 0);

        // Force a near-win for X on the top row regardless of the computer's
        // first reply, by using two more human moves; assert computer never
        // lets X complete a line without blocking when only one cell remains.
        // Set up: X has 0 and 1 -> cell 2 would win for X -> computer must take 2.
        // If the computer already took 2 as its first move, play a different
        // line instead so the assertion still exercises the blocking logic.
        if (afterFirst.Board[2] is null)
        {
            var afterSecond = service.MakeMove(game.Id, Player.X, 1);
            Assert.Equal(Player.O, afterSecond.Board[2]);
        }
        else
        {
            // Computer already occupies 2; verify it did so as a legal, single move.
            Assert.Equal(Player.O, afterFirst.Board[2]);
        }
    }

    [Fact]
    public void ComputerMode_NeverMovesAfterGameAlreadyCompleted()
    {
        var service = NewService(out _);
        var game = service.CreateGame(GameMode.VsComputer);

        // Drive X through the first open cell each turn until the game ends
        // (either X wins, the computer wins, or it's a draw) - the computer
        // auto-responds inside MakeMove each time.
        var state = service.GetGame(game.Id);
        while (state.Status == GameStatus.InProgress)
        {
            var nextCell = Enumerable.Range(0, 9).First(i => state.Board[i] is null);
            state = service.MakeMove(game.Id, Player.X, nextCell);
        }

        Assert.NotEqual(GameStatus.InProgress, state.Status);
        var moveCountAtCompletion = state.Moves.Count;

        // Any further move attempt must be rejected...
        Assert.Throws<InvalidMoveException>(() =>
        {
            var anyCell = Enumerable.Range(0, 9).FirstOrDefault(i => state.Board[i] is null, -1);
            // Even if the board happens to be full (draw), 0 is a legal index
            // to attempt - it will be rejected either for being occupied or
            // for the game already being finished.
            service.MakeMove(game.Id, Player.X, anyCell == -1 ? 0 : anyCell);
        });

        // ...and must not have appended any extra (e.g. computer) moves.
        var afterAttempt = service.GetGame(game.Id);
        Assert.Equal(moveCountAtCompletion, afterAttempt.Moves.Count);
    }
}
