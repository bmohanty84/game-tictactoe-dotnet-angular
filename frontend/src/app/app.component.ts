import { Component, OnInit } from '@angular/core';
import { GameService } from './services/game.service';
import { GameMode, GameState, Scoreboard } from './models/game.models';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  game: GameState | null = null;
  scoreboard: Scoreboard = { xWins: 0, oWins: 0, draws: 0 };
  errorMessage: string | null = null;
  loading = false;

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    this.startNewGame('TwoPlayer');
    this.refreshScoreboard();
  }

  startNewGame(mode: GameMode): void {
    this.loading = true;
    this.errorMessage = null;
    this.gameService.createGame(mode).subscribe({
      next: (state) => {
        this.game = state;
        this.loading = false;
      },
      error: () => this.handleError('Could not start a new game. Is the backend running?'),
    });
  }

  onModeChange(mode: GameMode): void {
    // Switching mode starts a brand new game (a mode change mid-game would
    // leave move history/turn semantics ambiguous), matching how the mode
    // selector is expected to behave in the problem statement.
    this.startNewGame(mode);
  }

  onCellClicked(cellIndex: number): void {
    if (!this.game) return;
    const player = this.game.currentPlayer;

    this.errorMessage = null;
    this.gameService.makeMove(this.game.id, player, cellIndex).subscribe({
      next: (state) => (this.game = state),
      error: (err) => this.handleError(this.extractMessage(err, 'That move was rejected.')),
      complete: () => this.refreshScoreboardIfGameJustEnded(),
    });
  }

  onUndo(): void {
    if (!this.game) return;
    this.errorMessage = null;
    this.gameService.undo(this.game.id).subscribe({
      next: (state) => (this.game = state),
      error: (err) => this.handleError(this.extractMessage(err, 'Undo is not available right now.')),
    });
  }

  onResetGame(): void {
    if (!this.game) return;
    this.errorMessage = null;
    this.gameService.resetGame(this.game.id).subscribe({
      next: (state) => (this.game = state),
      error: () => this.handleError('Could not reset the game.'),
    });
  }

  onResetScoreboard(): void {
    this.gameService.resetScoreboard().subscribe({
      next: (board) => (this.scoreboard = board),
      error: () => this.handleError('Could not reset the scoreboard.'),
    });
  }

  private refreshScoreboard(): void {
    this.gameService.getScoreboard().subscribe({
      next: (board) => (this.scoreboard = board),
      error: () => this.handleError('Could not load the scoreboard.'),
    });
  }

  // A move can end the game (win/draw), which updates the backend
  // scoreboard - re-fetch it after every move so the UI stays in sync.
  private refreshScoreboardIfGameJustEnded(): void {
    if (this.game && this.game.status !== 'InProgress') {
      this.refreshScoreboard();
    }
  }

  private handleError(fallbackMessage: string): void {
    this.errorMessage = fallbackMessage;
    this.loading = false;
  }

  private extractMessage(err: unknown, fallback: string): string {
    const httpError = err as { error?: { message?: string } };
    return httpError?.error?.message ?? fallback;
  }
}
