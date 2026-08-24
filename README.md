
# Tic Tac Toe — Full Stack Application

A complete Tic Tac Toe game with a .NET Web API backend and Angular frontend, featuring Two Player mode, Computer mode with three difficulty levels (Easy / Medium / Hard), a per-move 10-second countdown timer, undo, scoreboard, and real-time game state management.

---

## 1. Project Overview

This application lets two users play Tic Tac Toe on one screen, or a single user play against the computer at three difficulty levels:

| Mode | Description |
|------|-------------|
| Two Player | X and O are both controlled by users on the same screen |
| Computer – Easy | Computer mostly plays random moves — easy to beat |
| Computer – Medium | Computer follows a classic priority ladder (win → block → center → corner → any) |
| Computer – Hard | Computer uses perfect Minimax — it never loses |

The human always plays **X**; the computer plays **O** and moves automatically immediately after every human move. Each human move must be made within **10 seconds** or a random move is auto-played.

## 2. Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 10 Web API, C# |
| Frontend | Angular 22, TypeScript |
| Communication | REST API (JSON) |
| Storage | In-memory (`ConcurrentDictionary`) |
| Testing (Backend) | xUnit, `Microsoft.AspNetCore.Mvc.Testing` |
| Testing (Frontend) | Vitest, Angular Testing Utilities |

## 3. Features Implemented

- **Two Player Mode** — Play X vs O on the same screen
- **Computer Mode** — Human = X, computer = O, auto-move after every human move
- **Three Difficulty Levels** — Easy (mostly random), Medium (priority ladder), Hard (perfect Minimax)
- **10-Second Move Timer** — Visible countdown per turn; resets after every valid move; auto-plays a random move at 0
- **Win Detection** — Rows, columns, and both diagonals
- **Draw Detection** — All 9 cells filled with no winner
- **Winning Cell Highlighting** — Green animated highlight on winning cells
- **Move History** — Full move log with player, position, and move number
- **Undo Support** — Undo last move (removes both player + computer moves in Computer mode); disabled after game completion
- **Scoreboard** — Tracks X wins, O wins, and draws across games
- **Reset Game** — Clears the board while keeping the same game ID
- **Reset Scoreboard** — Zeros out all scores
- **Invalid Move Protection** — Rejects occupied cells, wrong player turns, moves after game completion
- **Responsive UI** — Dark gradient theme, mobile-friendly layout
- **Loading Protection & Error Handling** — Prevents duplicate clicks during API calls; displays API error messages

### Computer AI Priority Logic

All modes make only valid moves and never move after the game is completed.

**Medium** follows this exact priority:
1. If O can win → play the winning move
2. If X can win next → block X
3. Take center if available
4. Take a corner if available
5. Take any available cell

**Hard** uses full Minimax with depth-aware scoring (prefers faster wins, slower losses) — unbeatable; best a perfect opponent can achieve is a draw.

## 4. How to Run the Backend Locally

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
# From the repository root
dotnet build

cd TicTacToe.Api
dotnet run
```

The API is available at **http://localhost:5000**.

> The production UI build is also served from the API's `wwwroot`, so you can play directly at http://localhost:5000 without running the Angular dev server.

## 5. How to Run the Frontend Locally

Prerequisites: [Node.js 20+](https://nodejs.org/) and npm

```bash
cd TicTacToe-UI
npm install
npm start
```

The Angular dev server is available at **http://localhost:4200** (CORS for `localhost:4200` is pre-configured in the API).

To rebuild the production bundle into the API's `wwwroot` instead:

```bash
cd TicTacToe-UI
npm run build
Copy-Item dist/TicTacToe-UI/browser/* ../TicTacToe.Api/wwwroot/
```

## 6. API Endpoint Summary

Base URL: `http://localhost:5000`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/games` | Create a new game (`gameMode`: 0=TwoPlayer, 1=Computer; optional `difficulty`: 0=Easy, 1=Medium, 2=Hard) |
| GET | `/api/games/{id}` | Get current game state |
| POST | `/api/games/{id}/moves` | Make a move |
| POST | `/api/games/{id}/undo` | Undo last move |
| POST | `/api/games/{id}/reset` | Reset game board (same game ID) |
| GET | `/api/scoreboard` | Get scoreboard |
| POST | `/api/scoreboard/reset` | Reset scoreboard |

### Example Requests

```bash
# Create Two Player game
curl -X POST http://localhost:5000/api/games \
  -H "Content-Type: application/json" \
  -d '{"gameMode": 0}'

# Create Computer game (Hard)
curl -X POST http://localhost:5000/api/games \
  -H "Content-Type: application/json" \
  -d '{"gameMode": 1, "difficulty": 2}'

# Make a move (X at row 0, col 0) — computer replies automatically in Computer mode
curl -X POST http://localhost:5000/api/games/{gameId}/moves \
  -H "Content-Type: application/json" \
  -d '{"player": 0, "row": 0, "column": 0}'

# Undo
curl -X POST http://localhost:5000/api/games/{gameId}/undo

# Reset game
curl -X POST http://localhost:5000/api/games/{gameId}/reset

# Get scoreboard
curl http://localhost:5000/api/scoreboard

# Reset scoreboard
curl -X POST http://localhost:5000/api/scoreboard/reset
```

## 7. How to Run Tests

```bash
# Backend tests (39 tests: unit + integration)
dotnet test

# Frontend tests
cd TicTacToe-UI
npx ng test
```

## 8. AI Tools and Prompt Summary

This project was built using AI-assisted development. Key prompts used during development included:

1. *"Create an in-memory Tic Tac Toe backend with clean architecture"* — generated the layered .NET API (Controllers/Services/Interfaces/Models/DTOs)
2. *"Build the Angular UI for the game"* — generated the standalone Angular component with signals
3. *"Computer should block the human sometimes, and must win when it can; add a 10-second move timer"* — replaced random AI with Minimax and added the per-move countdown
4. *"Add Easy/Medium/Hard selection to Computer mode; Easy mostly random, Medium win/block/basic strategy, Hard perfect Minimax"* — added the `Difficulty` enum end-to-end
5. *"Medium should follow priority: win, block, center, corner, any"* — refined Medium to the exact ladder
6. *"Add a visible 10-second countdown per move; reset after each valid move"* — timer UX in the status bar

AI tools used: Codebuff/Freebuff coding agents (Claude-based models) inside an opencode-style CLI workflow.

## 9. Design Decisions

- **`Player?[,]` nullable board** — avoids `Player.X = 0` colliding with .NET default values for empty cells
- **`ConcurrentDictionary<Guid, Game>`** — thread-safe in-memory storage with no external dependencies
- **Singleton services via DI** — appropriate for in-memory state; interfaces (`IGameService`, `IComputerPlayerService`, …) allow swapping in database-backed implementations
- **Backend owns all game state** — the frontend is a pure renderer; no game logic duplicated in Angular. The computer's reply is applied server-side in the same request as the human move
- **Difficulty passed per game, stored on the `Game` model** — level is fixed at creation so games are reproducible
- **Timer lives entirely in the frontend** — it drives UX only; enforcement reuses existing valid-move logic (a random valid move is submitted through the normal endpoint)
- **Minimax depth scoring (`10 - depth`)** — makes Hard prefer quicker wins and delay losses
- **Undo disabled after game completion** — scoreboard stays final once a result is recorded

## 10. Clarifications and Assumptions

- Human is always X and always moves first in Computer mode
- The API defaults to `http://localhost:5000`; the dev UI to `http://localhost:4200`
- In-memory storage is acceptable — no persistence across API restarts
- The 10-second limit applies to human turns; when it expires, a random valid move is played for that player rather than forfeiting the game
- Easy still takes an obvious immediate win if it stumbles onto one, otherwise it plays randomly
- Medium is deterministic (no randomness beyond "any available cell" tie-breaking)
- Single-server deployment; CORS opened only for the Angular dev origin

## 11. Known Limitations

- In-memory storage means all games and scores are lost on API restart
- No authentication or user management
- Single-server deployment only (no horizontal scaling)
- The 10-second timer runs client-side only — API requests themselves are not time-limited
- CSS budget warning on production build (cosmetic only)
- Undo cannot revert past a completed game

## 12. Future Improvements

- Database persistence (SQLite or PostgreSQL)
- WebSocket / SignalR for real-time multiplayer across devices
- Server-side move-time enforcement (reject late moves via timestamps)
- User authentication, profiles, and persisted stats
- Game replay functionality
- Sound effects and richer animations
- Alpha-beta pruning benchmarking / larger board support
# tic_tac_toe

