import { Component, EventEmitter, Input, Output } from '@angular/core';
import { PlayerSymbol } from '../../models/game.models';

@Component({
  selector: 'app-board',
  templateUrl: './board.component.html',
  styleUrls: ['./board.component.css'],
})
export class BoardComponent {
  /** 9-element array: 'X', 'O', or null for an empty cell. */
  @Input() board: (PlayerSymbol | null)[] = Array(9).fill(null);

  /** Cell indices (0-8) that make up the winning line, if any. */
  @Input() winningCells: number[] | null = null;

  /** Whether clicking cells should currently be allowed at all. */
  @Input() interactive = true;

  @Output() cellClicked = new EventEmitter<number>();

  onCellClick(index: number): void {
    if (!this.interactive) return;
    if (this.board[index] !== null) return; // already occupied - ignore
    this.cellClicked.emit(index);
  }

  isWinningCell(index: number): boolean {
    return !!this.winningCells && this.winningCells.includes(index);
  }

  rowOf(index: number): number {
    return Math.floor(index / 3);
  }

  colOf(index: number): number {
    return index % 3;
  }
}
