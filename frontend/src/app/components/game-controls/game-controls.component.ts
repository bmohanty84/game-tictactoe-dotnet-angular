import { Component, EventEmitter, Input, Output } from '@angular/core';
import { GameMode, GameStatus, PlayerSymbol } from '../../models/game.models';

@Component({
  selector: 'app-game-controls',
  templateUrl: './game-controls.component.html',
  styleUrls: ['./game-controls.component.css'],
})
export class GameControlsComponent {
  @Input() mode: GameMode = 'TwoPlayer';
  @Input() status: GameStatus = 'InProgress';
  @Input() currentPlayer: PlayerSymbol = 'X';
  @Input() winner: PlayerSymbol | null = null;
  @Input() canUndo = false;

  @Output() modeChange = new EventEmitter<GameMode>();
  @Output() undo = new EventEmitter<void>();
  @Output() resetGame = new EventEmitter<void>();

  onModeSelected(mode: GameMode): void {
    if (mode === this.mode) return;
    this.modeChange.emit(mode);
  }

  onUndo(): void {
    this.undo.emit();
  }

  onReset(): void {
    this.resetGame.emit();
  }

  get statusMessage(): string {
    if (this.status === 'Won') {
      return `${this.winner} wins!`;
    }
    if (this.status === 'Draw') {
      return "It's a draw!";
    }
    return `${this.currentPlayer}'s turn`;
  }
}
