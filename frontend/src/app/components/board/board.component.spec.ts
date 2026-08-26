import { CommonModule } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BoardComponent } from './board.component';

describe('BoardComponent', () => {
  let fixture: ComponentFixture<BoardComponent>;
  let component: BoardComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [BoardComponent],
      imports: [CommonModule],
    });
    fixture = TestBed.createComponent(BoardComponent);
    component = fixture.componentInstance;
  });

  it('emits cellClicked with the index when an empty, interactive cell is clicked', () => {
    component.board = Array(9).fill(null);
    component.interactive = true;
    fixture.detectChanges();

    spyOn(component.cellClicked, 'emit');
    component.onCellClick(4);

    expect(component.cellClicked.emit).toHaveBeenCalledWith(4);
  });

  it('does not emit when the target cell is already occupied', () => {
    component.board = ['X', null, null, null, null, null, null, null, null];
    component.interactive = true;
    fixture.detectChanges();

    spyOn(component.cellClicked, 'emit');
    component.onCellClick(0);

    expect(component.cellClicked.emit).not.toHaveBeenCalled();
  });

  it('does not emit when the board is not interactive (e.g. game over)', () => {
    component.board = Array(9).fill(null);
    component.interactive = false;
    fixture.detectChanges();

    spyOn(component.cellClicked, 'emit');
    component.onCellClick(0);

    expect(component.cellClicked.emit).not.toHaveBeenCalled();
  });

  it('identifies winning cells correctly', () => {
    component.winningCells = [0, 1, 2];
    expect(component.isWinningCell(1)).toBeTrue();
    expect(component.isWinningCell(5)).toBeFalse();
  });
});
