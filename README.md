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

Base URL: `http://localhost:5080/api`

All endpoints use JSON. Enum values are serialized as strings. Every game
endpoint requires the game id returned by `POST /api/games`.

| Method | Endpoint | Purpose |
|---|---|---|
| `POST` | `/api/games` | Create a new game session. |
| `GET` | `/api/games/{id}` | Return the current state of a game. |
| `POST` | `/api/games/{id}/moves` | Submit a move and, in computer mode, apply the computer response. |
| `POST` | `/api/games/{id}/undo` | Undo the latest move, or the latest human/computer pair in computer mode. |
| `POST` | `/api/games/{id}/reset` | Clear the board and move history while preserving the game id and scoreboard. |
| `GET` | `/api/scoreboard` | Return the process-wide session scoreboard. |
| `POST` | `/api/scoreboard/reset` | Reset all scoreboard counters to zero. |

### Create a game

`POST /api/games`

Request:

```json
{ "mode": "TwoPlayer" }
```

`mode` may be `TwoPlayer` or `VsComputer`. The response is `201 Created` and
contains the new game state.

### Get game state

`GET /api/games/{id}`

Returns `200 OK` with the current game state. An unknown or invalid GUID
returns `404 Not Found`.

### Submit a move

`POST /api/games/{id}/moves`

Request using a cell index:

```json
{ "player": "X", "cellIndex": 0 }
```

Alternatively, provide row and column values:

```json
{ "player": "X", "row": 0, "col": 0 }
```

`cellIndex` is zero-based (`0` through `8`) and takes precedence when both
forms are supplied. Row and column values are zero-based (`0` through `2`).
The response is `200 OK` with the updated game state. In `VsComputer` mode,
the backend adds the computer move before returning the response.

Invalid moves return `400 Bad Request`; an unknown game returns `404 Not
Found`. Both responses use the following shape:

```json
{ "message": "A descriptive error message" }
```

### Undo a move

`POST /api/games/{id}/undo`

The request body is empty (`{}`). The response is `200 OK` with the updated
game state. Undo is unavailable after a win or draw, and when there are no
moves to remove; those cases return `400 Bad Request`.

### Reset a game

`POST /api/games/{id}/reset`

The request body is empty (`{}`). The response is `200 OK` with a fresh,
in-progress game state using the same id and mode. The scoreboard is unchanged.

### Read or reset the scoreboard

`GET /api/scoreboard` returns `200 OK`:

```json
{ "xWins": 0, "oWins": 0, "draws": 0 }
```

`POST /api/scoreboard/reset` accepts an empty body (`{}`) and returns `200 OK`
with the reset scoreboard.

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

## 9. AI Prompt Summary and Workflow Notes

The project was developed with an AI pair-programming assistant. The prompts
below summarize the workflow and can be used to reproduce the implementation.
They are paraphrased rather than copied verbatim.

### Prompt 1: Convert requirements into a design

> Analyze the Tic Tac Toe requirements. Identify the domain model, game modes,
> move and undo rules, computer-player priority, scoreboard behavior, REST
> endpoints, validation rules, and test cases. Keep the backend as the single
> source of truth and use in-memory storage.

The output was reviewed against the problem statement before implementation.
Key decisions were to use a `GameSession`, explicit status and player enums,
move records, a process-wide scoreboard, and a service boundary between the
controllers and game rules.

### Prompt 2: Implement the .NET backend

> Create a .NET 8 Web API for the approved design. Add models, DTOs, game
> rules, services, controllers, Swagger, CORS for Angular on port 4200, and
> readable JSON enum values. Validate all invalid moves and return the
> documented error shape. Make computer moves synchronously after a human
> move.

The backend was then checked for route consistency, thread-safe in-memory
state, scoreboard updates exactly once per completed game, and correct
computer priority: win, block, center, corner, then the first available cell.

### Prompt 3: Implement the Angular frontend

> Create an Angular 17 frontend that calls the REST API exclusively. Add a
> board, game controls, move history, scoreboard, service models, and tests.
> Do not duplicate turn, win, draw, computer, or undo logic in the client;
> render the state returned by the backend.

The frontend was organized into small components and a single API service.
The service uses the backend base URL, while `AppComponent` coordinates game
creation, moves, reset actions, undo, and scoreboard refreshes.

### Prompt 4: Add and review tests

> Add focused xUnit tests for every game-rule and service requirement,
> including invalid moves, all win lines, draws, reset, both undo modes,
> scoreboard updates, and computer move priority. Add Angular unit tests for
> component rendering and every HTTP request made by the service and app.

During review, the completion test for computer mode was changed to drive the
game to completion dynamically. This avoids relying on a hardcoded sequence
that could become invalid if the computer's board-dependent choices change.

### Prompt 5: Verify and document

> Review the implementation against the requirements. Identify missing edge
> cases, API contract mismatches, concurrency risks, and test gaps. Update the
> README with run commands, endpoint documentation, assumptions, limitations,
> design decisions, and this AI workflow summary.

The remaining assumptions, trade-offs, and known limitations are recorded in
sections 10 through 13. The recommended final check is to run `dotnet test`,
start the API, run the Angular tests, and exercise the application through
the browser and Swagger UI.

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
- The computer opponent implements the specified fixed-priority heuristic
  (win/block/center/corner/any) rather than a full minimax search, so it is
  not unbeatable — matching "Basic Computer Mode" as scoped in the problem
  statement.

## 14. Future Improvements

- Add SQLite persistence behind the existing `IGameService`/`IScoreboardService`
  interfaces so game history survives a restart.
- Add a minimax-based "hard" difficulty option alongside the current
  heuristic-based computer player.
- Add E2E tests covering a full play-through in a real browser.
- Add optimistic UI updates on the frontend (render the human's move
  immediately, then reconcile with the backend response) to mask network
  latency.
- Support named/multiple concurrent game sessions in the UI (currently the
  frontend only ever tracks one active game at a time).
