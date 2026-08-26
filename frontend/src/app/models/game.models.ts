export type PlayerSymbol = 'X' | 'O';

export type GameMode = 'TwoPlayer' | 'VsComputer';

export type GameStatus = 'InProgress' | 'Won' | 'Draw';

export interface MoveDto {
  moveNumber: number;
  player: PlayerSymbol;
  row: number;
  col: number;
  cellIndex: number;
  isComputerMove: boolean;
}

/**
 * Mirrors the backend's GameStateResponse. The backend is the single source
 * of truth for all of this - the frontend never derives game rules itself,
 * it only renders whatever this object contains.
 */
export interface GameState {
  id: string;
  board: (PlayerSymbol | null)[];
  currentPlayer: PlayerSymbol;
  mode: GameMode;
  status: GameStatus;
  winner: PlayerSymbol | null;
  winningCells: number[] | null;
  moves: MoveDto[];
  canUndo: boolean;
}

export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface ApiError {
  message: string;
}
