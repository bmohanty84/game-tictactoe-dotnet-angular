using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Pure, stateless Tic Tac Toe rules: win-line detection and the computer
/// opponent's move selection. Kept free of GameSession/mutation so it's
/// trivial to unit test in isolation from the in-memory store.
/// </summary>
public static class GameRules
{
    // All 8 possible winning lines, expressed as cell indices (0-8).
    public static readonly int[][] WinningLines =
    {
        new[] { 0, 1, 2 }, // rows
        new[] { 3, 4, 5 },
        new[] { 6, 7, 8 },
        new[] { 0, 3, 6 }, // columns
        new[] { 1, 4, 7 },
        new[] { 2, 5, 8 },
        new[] { 0, 4, 8 }, // diagonals
        new[] { 2, 4, 6 }
    };

    private static readonly int[] Corners = { 0, 2, 6, 8 };
    private const int Center = 4;

    /// <summary>
    /// Returns the winning line (3 cell indices) if the given board has a
    /// winner, otherwise null. Checks the whole board rather than just the
    /// last move, so it also works correctly after an Undo recalculation.
    /// </summary>
    public static int[]? FindWinningLine(Player?[] board)
    {
        foreach (var line in WinningLines)
        {
            var a = board[line[0]];
            var b = board[line[1]];
            var c = board[line[2]];
            if (a is not null && a == b && b == c)
            {
                return line;
            }
        }
        return null;
    }

    public static bool IsBoardFull(Player?[] board) => board.All(cell => cell is not null);

    /// <summary>
    /// Computer move priority, exactly as specified:
    /// 1. Win if possible
    /// 2. Block opponent's win
    /// 3. Take center
    /// 4. Take a corner
    /// 5. Take any available cell
    /// </summary>
    public static int SelectComputerMove(Player?[] board, Player computer)
    {
        var human = computer.Other();

        var winningMove = FindWinningMove(board, computer);
        if (winningMove is not null) return winningMove.Value;

        var blockingMove = FindWinningMove(board, human);
        if (blockingMove is not null) return blockingMove.Value;

        if (board[Center] is null) return Center;

        var openCorner = Corners.FirstOrDefault(i => board[i] is null, -1);
        if (openCorner != -1) return openCorner;

        var anyOpen = Enumerable.Range(0, 9).FirstOrDefault(i => board[i] is null, -1);
        if (anyOpen != -1) return anyOpen;

        throw new InvalidOperationException("SelectComputerMove called on a full board.");
    }

    /// <summary>
    /// Finds a cell that would complete a line for the given player if played
    /// right now, i.e. two cells of a line already belong to that player and
    /// the third is empty. Used both for "can I win" and "must I block".
    /// </summary>
    private static int? FindWinningMove(Player?[] board, Player player)
    {
        foreach (var line in WinningLines)
        {
            var cells = line.Select(i => board[i]).ToArray();
            var emptyIndexInLine = Array.IndexOf(cells, null);
            if (emptyIndexInLine == -1) continue;

            var filled = cells.Where(c => c is not null).ToArray();
            if (filled.Length == 2 && filled.All(c => c == player))
            {
                return line[emptyIndexInLine];
            }
        }
        return null;
    }
}
