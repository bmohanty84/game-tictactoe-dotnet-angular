import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Scoreboard } from '../../models/game.models';

@Component({
  selector: 'app-scoreboard',
  templateUrl: './scoreboard.component.html',
  styleUrls: ['./scoreboard.component.css'],
})
export class ScoreboardComponent {
  @Input() scoreboard: Scoreboard = { xWins: 0, oWins: 0, draws: 0 };
  @Output() resetScoreboard = new EventEmitter<void>();

  onReset(): void {
    this.resetScoreboard.emit();
  }
}
