import { Component, Input } from '@angular/core';
import { MoveDto } from '../../models/game.models';

@Component({
  selector: 'app-move-history',
  templateUrl: './move-history.component.html',
  styleUrls: ['./move-history.component.css'],
})
export class MoveHistoryComponent {
  @Input() moves: MoveDto[] = [];

  positionLabel(move: MoveDto): string {
    return `Row ${move.row + 1}, Column ${move.col + 1}`;
  }
}
