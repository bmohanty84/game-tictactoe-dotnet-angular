# Tic Tac Toe — Angular + .NET

A browser-based Tic Tac Toe game with an Angular frontend and a .NET 8 Web API
backend. The backend owns all game rules, session state, move history, and
the scoreboard; the frontend is a thin renderer that calls the API and
displays whatever it returns.

---

## 1. Project Overview

- Standard 3×3 Tic Tac Toe, playable as **Two Player Mode** or **Play Against
  Computer** (human is always X, computer is always O).
- Full move history, undo (mode-aware), win/draw detection with winning-cell
  highlighting, and a persistent session-level scoreboard.
- The Angular app talks to the .NET API exclusively over REST; there is no
  game logic duplicated on the client.

## 2. Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 17, TypeScript, Karma/Jasmine for tests |
| Backend | .NET 8, ASP.NET Core Web API, C# |
| API style | REST (JSON) |
| Storage | In-memory (`ConcurrentDictionary` for games, a single locked object for the scoreboard) |
| Backend tests | xUnit |
| Source control | GitHub |

No database is required — everything resets when the API process restarts,
which is acceptable per the problem statement ("in-memory storage is
acceptable").

## 3. Features Implemented

- [x] 3×3 board, click-to-play, cells lock once filled
- [x] Two Player Mode and Play Against Computer mode
- [x] Turn display and alternation; invalid moves never change the turn
- [x] Win detection (row, column, diagonal) with winning-cell highlighting
- [x] Draw detection
- [x] Reset Game (clears board/history/status, keeps scoreboard)
- [x] Move history (move #, player, row/column)
- [x] Undo Last Move, mode-aware:
  - Two Player Mode → removes only the most recent move
  - Computer Mode → removes the computer's move and the preceding human move together
  - Disabled once a game is won or drawn, and disabled when there's nothing to undo
- [x] Session-level scoreboard (X wins / O wins / draws), served by the backend, updated exactly once per completed game, unaffected by Reset Game, with a separate Reset Scoreboard action
- [x] Computer opponent following the required priority: win > block > center > corner > any available cell
- [x] Backend rejects invalid moves: out-of-range cell, occupied cell, move after completion, wrong player's turn
- [x] Unit tests for backend game logic (xUnit) and frontend components/services (Jasmine/Karma)

## 4. Project Structure

```
tictactoe/
├── backend/
│   ├── TicTacToe.Api/            .NET Web API project
│   └── TicTacToe.Api.Tests/      xUnit test project
├── frontend/
│   └── src/app/                  Angular application
└── README.md
```

## 5. How to Run the Backend Locally

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
cd backend/TicTacToe.Api
dotnet restore
dotnet run
```

The API starts on **http://localhost:5080** (configured in
`Properties/launchSettings.json`) and opens a Swagger UI at
`http://localhost:5080/swagger` for exploring/testing endpoints directly.

CORS is pre-configured to allow requests from `http://localhost:4200` (the
Angular dev server's default port).

## 6. How to Run the Frontend Locally

Requires [Node.js](https://nodejs.org/) 18+ and npm.

```bash
cd frontend
npm install
npm start
```

This runs `ng serve` and opens the app at **http://localhost:4200**. Make
sure the backend (step 5) is already running — the frontend calls it at
`http://localhost:5080/api`. If you need to run the API on a different port,
update the `API_BASE` constant in `src/app/services/game.service.ts`.

## 7. API Endpoint Summary

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/games` | Create a new game session. Body: `{ "mode": "TwoPlayer" \| "VsComputer" }` |
| GET | `/api/games/{id}` | Get current game state |
| POST | `/api/games/{id}/moves` | Submit a move. Body: `{ "player": "X"\|"O", "cellIndex": 0-8 }` (or `row`/`col` instead of `cellIndex`) |
| POST | `/api/games/{id}/undo` | Undo the last move (mode-aware) |
| POST | `/api/games/{id}/reset` | Reset the current game (scoreboard untouched) |
| GET | `/api/scoreboard` | Get the session-level scoreboard |
| POST | `/api/scoreboard/reset` | Reset the scoreboard to zero |

**Game state response shape:**
```json
{
  "id": "guid",
  "board": ["X", null, "O", null, "X", null, null, null, null],
  "currentPlayer": "O",
  "mode": "TwoPlayer",
  "status": "InProgress",
  "winner": null,
  "winningCells": null,
  "moves": [
    { "moveNumber": 1, "player": "X", "row": 0, "col": 0, "cellIndex": 0, "isComputerMove": false }
  ],
  "canUndo": true
}
```

`status` is one of `InProgress`, `Won`, `Draw`. Invalid requests return
`400 Bad Request` (bad move/undo) or `404 Not Found` (unknown game id), each
with `{ "message": "..." }`.

Full interactive documentation is also available via Swagger UI once the
backend is running (see section 5).

## 8. How to Run Tests

**Backend (xUnit):**
```bash
cd backend
dotnet test
```

**Frontend (Karma/Jasmine):**
```bash
cd frontend
npm test
```
(`npm test` runs headless Chrome via `karma.conf.js`; a Chrome/Chromium
binary must be available on the machine, or set `CHROME_BIN` to point to
one.)

## 9. AI Tools and Prompt Summary

This solution was built with Claude (Anthropic) as an AI pair-programmer,
inside a single continuous session. Summary of the workflow for
transparency, as requested by the assignment:

- **Specification conversion:** The uploaded problem statement (Word doc)
  was parsed directly, and the functional requirements, undo semantics,
  computer-move priority, and API contract were extracted into the data
  model and service design below before any code was written.
- **Prompts used (paraphrased):** "Analyze the requirements and generate
  source code" was the driving prompt; the assistant then made and stated
  the specific design decisions in this README rather than asking for
  every micro-decision to be re-confirmed.
- **What the AI generated:** The full backend (models, DTOs, services,
  controllers, xUnit tests) and full frontend (Angular module, components,
  services, Karma tests), plus this README.
- **What was changed/refined manually during generation:** The undo-related
  xUnit test for "computer never moves after completion" was rewritten
  mid-generation to drive the game to completion deterministically instead
  of hardcoding a move sequence that could vary depending on the computer's
  play, which is a more reliable test given the computer's fixed-priority
  but board-dependent behavior.
- **What was reviewed carefully:** The undo-by-mode logic (single move vs.
  move-pair removal) and the computer's win/block/center/corner/any-cell
  priority chain, since both have several edge cases (e.g., undoing when
  only one move has been made in Computer Mode).
- **Assumptions made:** documented in section 10 below.
- **Trade-offs chosen:** documented in section 11 (Design Decisions) below.

Because this was authored with an AI assistant, **please run and exercise
the app yourself before the panel review** to confirm behavior end-to-end
in your actual environment — the code hasn't been compiled/executed in the
authoring environment (no .NET SDK / npm registry access there); see
section 13, Known Limitations.

## 10. Clarifications and Assumptions

- **Clarification 2 (Scoreboard vs Undo) — chosen approach: Option A.**
  Undo is disabled once a game is `Won` or `Draw`. This keeps the
  scoreboard simple and always consistent with completed-game state,
  at the cost of not being able to take back a finished game's result.
- The backend is the sole source of truth (Clarification 1); the frontend
  never computes win/draw/turn logic itself, it only renders API responses.
- In Computer Mode, the human is always X and the computer is always O, per
  the problem statement. The API rejects a client attempting to submit a
  move as O in that mode (`InvalidMoveException`).
- A move request may supply either `cellIndex` (0-8) or `row`/`col` (each
  0-2); the frontend uses `cellIndex`. `cellIndex` takes precedence if both
  are supplied.
- **Reset Game** keeps the same game `Id` and `Mode`, only clearing board,
  history, and status — so any client already holding that game id keeps
  working after a reset.
- The scoreboard is a single, process-wide counter rather than one per game
  session, matching the "session-level scoreboard...served by the backend"
  requirement as a shared running tally across games played in that run of
  the app.
- When multiple cells tie for the computer's "any available cell" step, the
  lowest-index open cell is chosen, to keep computer behavior deterministic
  and testable (the requirement doesn't specify tie-breaking).

## 11. Design Decisions

- **In-memory storage via `ConcurrentDictionary<Guid, GameSession>`** rather
  than SQLite — the problem statement calls this acceptable, and it keeps
  the reviewable surface area smaller. Swapping in EF Core + SQLite would
  only require changing `GameService`'s storage calls behind the existing
  `IGameService` interface.
- **Win/draw evaluation recomputes from the whole board** (`GameRules.FindWinningLine`)
  rather than checking only the lines through the last-played cell. This is
  slightly more work per move (at most 8 line checks on a 9-cell board —
  negligible) but means the same function can be reused for the defensive
  recalculation after Undo without any special-casing.
- **The computer's reply is applied synchronously inside the same
  `POST /moves` call** that submits the human's move, rather than requiring
  the frontend to poll or make a second request. This keeps the frontend
  simple (one round trip per human turn) and matches "computer should make
  a move automatically after the human move."
- **`GameRules` is a stateless, static class** separate from `GameService`
  specifically so win-detection and computer-move-selection can be unit
  tested directly against arbitrary boards, without needing to drive a full
  game session through the service.
- **Locking:** each `GameSession` object is locked around its own
  read-modify-write in `GameService`, and the scoreboard has its own lock,
  since ASP.NET Core can serve concurrent requests on different threads for
  what's otherwise a simple in-memory demo.

## 12. Testing Approach

Backend unit tests (`TicTacToe.Api.Tests`) cover, per the assignment's
minimum list: valid move, invalid move (out-of-range, occupied, wrong
turn), turn switching, row/column/diagonal win, draw, move after
completion, reset, undo in both modes (including "no moves to undo" and
"undo disabled after completion"), scoreboard update (including
"updates exactly once"), scoreboard reset, and computer move selection
(win/block/center/corner priority, and "never moves after game ends").

Frontend tests cover component rendering (`BoardComponent`,
`ScoreboardComponent`, `GameControlsComponent`) and API integration points
(`GameService`'s HTTP calls, and `AppComponent`'s end-to-end wiring of
create → move → scoreboard refresh, using `HttpClientTestingModule`).

## 13. Known Limitations

- No database/persistence — restarting the backend process clears all
  games and the scoreboard.
- No authentication/multi-user isolation — this is a single local session,
  as scoped by the problem statement.
- No E2E (Cypress/Playwright) tests — only unit tests, per "frontend tests
  may cover component rendering and API integration points."
- **This code was generated in an environment without the .NET SDK or npm
  registry access, so it has not actually been compiled or run there.**
  Run `dotnet build` / `dotnet test` and `npm install` / `npm test` locally
  as the first step, and fix up any small compilation issues (e.g. NuGet
  package version availability) you encounter — the logic and structure
  are complete, but a first local build is recommended before the panel
  review.
- The computer opponent implements the specified fixed-priority heuristic
  (win/block/center/corner/any) rather than a full minimax search, so it is
  not unbeatable — matching "Basic Computer Mode" as scoped in the problem
  statement.

## 14. Future Improvements

- Add SQLite persistence behind the existing `IGameService`/`IScoreboardService`
  interfaces so game history survives a restart.
- Add a minimax-based "hard" difficulty option alongside the current
  heuristic-based computer player.
- Add E2E tests (Playwright) covering a full play-through in a real browser.
- Add optimistic UI updates on the frontend (render the human's move
  immediately, then reconcile with the backend response) to mask network
  latency.
- Support named/multiple concurrent game sessions in the UI (currently the
  frontend only ever tracks one active game at a time).
