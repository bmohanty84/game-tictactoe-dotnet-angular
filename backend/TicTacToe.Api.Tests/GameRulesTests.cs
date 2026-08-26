using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Api.Tests;

public class GameRulesTests
{
    private static Player?[] EmptyBoard() => new Player?[9];

    [Fact]
    public void SelectComputerMove_TakesWinningMoveWhenAvailable()
    {
        var board = EmptyBoard();
        // O has 0 and 1 -> 2 completes the top row for O.
        board[0] = Player.O;
        board[1] = Player.O;
        board[3] = Player.X;
        board[4] = Player.X;

        var move = GameRules.SelectComputerMove(board, Player.O);

        Assert.Equal(2, move);
    }

    [Fact]
    public void SelectComputerMove_BlocksOpponentWinWhenCannotWinItself()
    {
        var board = EmptyBoard();
        // X has 0 and 1 -> would win at 2 next turn. O has no winning move available.
        board[0] = Player.X;
        board[1] = Player.X;
        board[6] = Player.O;

        var move = GameRules.SelectComputerMove(board, Player.O);

        Assert.Equal(2, move);
    }

    [Fact]
    public void SelectComputerMove_TakesCenterWhenNoWinOrBlockNeeded()
    {
        var board = EmptyBoard();
        board[0] = Player.X;

        var move = GameRules.SelectComputerMove(board, Player.O);

        Assert.Equal(4, move); // center
    }

    [Fact]
    public void SelectComputerMove_TakesCornerWhenCenterTaken()
    {
        var board = EmptyBoard();
        board[4] = Player.X; // center already occupied

        var move = GameRules.SelectComputerMove(board, Player.O);

        Assert.Contains(move, new[] { 0, 2, 6, 8 });
    }

    [Fact]
    public void SelectComputerMove_TakesAnyAvailableCellAsLastResort()
    {
        var board = EmptyBoard();
        // Fill center and all corners, leaving only edge cells (1, 3, 5, 7).
        board[4] = Player.X;
        board[0] = Player.X;
        board[2] = Player.O;
        board[6] = Player.X;
        board[8] = Player.O;

        var move = GameRules.SelectComputerMove(board, Player.O);

        Assert.Contains(move, new[] { 1, 3, 5, 7 });
    }

    [Theory]
    [InlineData(0, 1, 2)]
    [InlineData(3, 4, 5)]
    [InlineData(6, 7, 8)]
    [InlineData(0, 3, 6)]
    [InlineData(1, 4, 7)]
    [InlineData(2, 5, 8)]
    [InlineData(0, 4, 8)]
    [InlineData(2, 4, 6)]
    public void FindWinningLine_DetectsEveryLine(int a, int b, int c)
    {
        var board = EmptyBoard();
        board[a] = Player.X;
        board[b] = Player.X;
        board[c] = Player.X;

        var line = GameRules.FindWinningLine(board);

        Assert.NotNull(line);
        Assert.Equal(new[] { a, b, c }, line);
    }

    [Fact]
    public void FindWinningLine_ReturnsNullWhenNoWinner()
    {
        var board = EmptyBoard();
        board[0] = Player.X;
        board[1] = Player.O;

        Assert.Null(GameRules.FindWinningLine(board));
    }

    [Fact]
    public void IsBoardFull_TrueOnlyWhenEveryCellOccupied()
    {
        var board = EmptyBoard();
        Assert.False(GameRules.IsBoardFull(board));

        for (var i = 0; i < 9; i++) board[i] = i % 2 == 0 ? Player.X : Player.O;

        Assert.True(GameRules.IsBoardFull(board));
    }
}
