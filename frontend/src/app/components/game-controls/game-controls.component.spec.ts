import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GameControlsComponent } from './game-controls.component';

describe('GameControlsComponent', () => {
  let fixture: ComponentFixture<GameControlsComponent>;
  let component: GameControlsComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({ declarations: [GameControlsComponent] });
    fixture = TestBed.createComponent(GameControlsComponent);
    component = fixture.componentInstance;
  });

  it('shows whose turn it is while the game is in progress', () => {
    component.status = 'InProgress';
    component.currentPlayer = 'O';
    expect(component.statusMessage).toBe("O's turn");
  });

  it('shows the winner when the game has been won', () => {
    component.status = 'Won';
    component.winner = 'X';
    expect(component.statusMessage).toBe('X wins!');
  });

  it('shows a draw message when the game is drawn', () => {
    component.status = 'Draw';
    expect(component.statusMessage).toBe("It's a draw!");
  });

  it('emits modeChange only when a different mode is selected', () => {
    component.mode = 'TwoPlayer';
    spyOn(component.modeChange, 'emit');

    component.onModeSelected('TwoPlayer'); // same mode - no-op
    expect(component.modeChange.emit).not.toHaveBeenCalled();

    component.onModeSelected('VsComputer');
    expect(component.modeChange.emit).toHaveBeenCalledWith('VsComputer');
  });

  it('disables the Undo button when canUndo is false', () => {
    component.canUndo = false;
    fixture.detectChanges();

    const undoButton: HTMLButtonElement = fixture.nativeElement.querySelector('button');
    expect(undoButton.disabled).toBeTrue();
  });

  it('emits resetGame when Reset Game is triggered', () => {
    spyOn(component.resetGame, 'emit');
    component.onReset();
    expect(component.resetGame.emit).toHaveBeenCalled();
  });
});
