import { CommonModule } from '@angular/common';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { BoardComponent } from './components/board/board.component';
import { GameControlsComponent } from './components/game-controls/game-controls.component';
import { MoveHistoryComponent } from './components/move-history/move-history.component';
import { ScoreboardComponent } from './components/scoreboard/scoreboard.component';
import { GameState, Scoreboard } from './models/game.models';

describe('AppComponent', () => {
  let fixture: ComponentFixture<AppComponent>;
  let component: AppComponent;
  let httpMock: HttpTestingController;

  const freshGame: GameState = {
    id: 'game-1',
    board: Array(9).fill(null),
    currentPlayer: 'X',
    mode: 'TwoPlayer',
    status: 'InProgress',
    winner: null,
    winningCells: null,
    moves: [],
    canUndo: false,
  };

  const emptyScoreboard: Scoreboard = { xWins: 0, oWins: 0, draws: 0 };

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [
        AppComponent,
        BoardComponent,
        GameControlsComponent,
        MoveHistoryComponent,
        ScoreboardComponent,
      ],
      imports: [CommonModule, HttpClientTestingModule],
    });

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);

    fixture.detectChanges(); // triggers ngOnInit -> createGame + getScoreboard

    httpMock.expectOne('http://localhost:5080/api/games').flush(freshGame);
    httpMock.expectOne('http://localhost:5080/api/scoreboard').flush(emptyScoreboard);
  });

  afterEach(() => httpMock.verify());

  it('creates a new TwoPlayer game and loads the scoreboard on init', () => {
    expect(component.game).toEqual(freshGame);
    expect(component.scoreboard).toEqual(emptyScoreboard);
  });

  it('sends a move for the current player when a cell is clicked', () => {
    component.onCellClicked(0);

    const req = httpMock.expectOne('http://localhost:5080/api/games/game-1/moves');
    expect(req.request.body).toEqual({ player: 'X', cellIndex: 0 });

    const afterMove: GameState = {
      ...freshGame,
      board: ['X', null, null, null, null, null, null, null, null],
      currentPlayer: 'O',
      moves: [{ moveNumber: 1, player: 'X', row: 0, col: 0, cellIndex: 0, isComputerMove: false }],
      canUndo: true,
    };
    req.flush(afterMove);

    expect(component.game).toEqual(afterMove);
  });

  it('re-fetches the scoreboard once a move completes the game', () => {
    component.onCellClicked(0);
    const req = httpMock.expectOne('http://localhost:5080/api/games/game-1/moves');

    const wonState: GameState = { ...freshGame, status: 'Won', winner: 'X', canUndo: false };
    req.flush(wonState);

    const scoreReq = httpMock.expectOne('http://localhost:5080/api/scoreboard');
    scoreReq.flush({ xWins: 1, oWins: 0, draws: 0 });

    expect(component.scoreboard.xWins).toBe(1);
  });

  it('starting a new mode creates a brand new game', () => {
    component.onModeChange('VsComputer');

    const req = httpMock.expectOne('http://localhost:5080/api/games');
    expect(req.request.body).toEqual({ mode: 'VsComputer' });
    req.flush({ ...freshGame, mode: 'VsComputer' });

    expect(component.game?.mode).toBe('VsComputer');
  });

  it('shows a friendly error message when the backend is unreachable', () => {
    component.startNewGame('TwoPlayer');

    const req = httpMock.expectOne('http://localhost:5080/api/games');
    req.error(new ProgressEvent('network error'));

    expect(component.errorMessage).toContain('backend running');
  });
});
