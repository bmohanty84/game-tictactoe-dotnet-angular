import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { GameService } from './game.service';
import { GameState, Scoreboard } from '../models/game.models';

describe('GameService', () => {
  let service: GameService;
  let httpMock: HttpTestingController;

  const sampleState: GameState = {
    id: 'abc-123',
    board: Array(9).fill(null),
    currentPlayer: 'X',
    mode: 'TwoPlayer',
    status: 'InProgress',
    winner: null,
    winningCells: null,
    moves: [],
    canUndo: false,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [GameService],
    });
    service = TestBed.inject(GameService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('creates a game via POST /api/games with the selected mode', () => {
    service.createGame('VsComputer').subscribe((state) => {
      expect(state).toEqual(sampleState);
    });

    const req = httpMock.expectOne('http://localhost:5080/api/games');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ mode: 'VsComputer' });
    req.flush(sampleState);
  });

  it('submits a move via POST /api/games/{id}/moves', () => {
    service.makeMove('abc-123', 'X', 4).subscribe();

    const req = httpMock.expectOne('http://localhost:5080/api/games/abc-123/moves');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ player: 'X', cellIndex: 4 });
    req.flush(sampleState);
  });

  it('calls undo via POST /api/games/{id}/undo', () => {
    service.undo('abc-123').subscribe();

    const req = httpMock.expectOne('http://localhost:5080/api/games/abc-123/undo');
    expect(req.request.method).toBe('POST');
    req.flush(sampleState);
  });

  it('resets a game via POST /api/games/{id}/reset', () => {
    service.resetGame('abc-123').subscribe();

    const req = httpMock.expectOne('http://localhost:5080/api/games/abc-123/reset');
    expect(req.request.method).toBe('POST');
    req.flush(sampleState);
  });

  it('fetches the scoreboard via GET /api/scoreboard', () => {
    const board: Scoreboard = { xWins: 2, oWins: 1, draws: 0 };
    service.getScoreboard().subscribe((result) => expect(result).toEqual(board));

    const req = httpMock.expectOne('http://localhost:5080/api/scoreboard');
    expect(req.request.method).toBe('GET');
    req.flush(board);
  });

  it('resets the scoreboard via POST /api/scoreboard/reset', () => {
    const board: Scoreboard = { xWins: 0, oWins: 0, draws: 0 };
    service.resetScoreboard().subscribe((result) => expect(result).toEqual(board));

    const req = httpMock.expectOne('http://localhost:5080/api/scoreboard/reset');
    expect(req.request.method).toBe('POST');
    req.flush(board);
  });
});
