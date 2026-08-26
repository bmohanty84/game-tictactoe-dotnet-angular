using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IScoreboardService
{
    Scoreboard GetScoreboard();
    void RecordWin(Player winner);
    void RecordDraw();
    void Reset();
}

/// <summary>
/// Single in-memory scoreboard, guarded by a lock since ASP.NET Core can
/// process requests on multiple threads concurrently. Simple and sufficient
/// for a local/demo-scale app; a real deployment would back this with a
/// database or distributed cache instead.
/// </summary>
public class ScoreboardService : IScoreboardService
{
    private readonly Scoreboard _scoreboard = new();
    private readonly object _lock = new();

    public Scoreboard GetScoreboard()
    {
        lock (_lock)
        {
            // Return a copy so callers can't mutate internal state directly.
            return new Scoreboard
            {
                XWins = _scoreboard.XWins,
                OWins = _scoreboard.OWins,
                Draws = _scoreboard.Draws
            };
        }
    }

    public void RecordWin(Player winner)
    {
        lock (_lock)
        {
            if (winner == Player.X) _scoreboard.XWins++;
            else _scoreboard.OWins++;
        }
    }

    public void RecordDraw()
    {
        lock (_lock)
        {
            _scoreboard.Draws++;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _scoreboard.XWins = 0;
            _scoreboard.OWins = 0;
            _scoreboard.Draws = 0;
        }
    }
}
