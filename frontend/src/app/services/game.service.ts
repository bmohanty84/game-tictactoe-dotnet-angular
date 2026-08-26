import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GameMode, GameState, PlayerSymbol, Scoreboard } from '../models/game.models';

// Base URL of the local .NET backend. See README for how to change this if
// the API is run on a different port.
const API_BASE = 'http://localhost:5080/api';

@Injectable({ providedIn: 'root' })
export class GameService {
  constructor(private http: HttpClient) {}

  createGame(mode: GameMode): Observable<GameState> {
    return this.http.post<GameState>(`${API_BASE}/games`, { mode });
  }

  getGame(id: string): Observable<GameState> {
    return this.http.get<GameState>(`${API_BASE}/games/${id}`);
  }

  makeMove(id: string, player: PlayerSymbol, cellIndex: number): Observable<GameState> {
    return this.http.post<GameState>(`${API_BASE}/games/${id}/moves`, {
      player,
      cellIndex,
    });
  }

  undo(id: string): Observable<GameState> {
    return this.http.post<GameState>(`${API_BASE}/games/${id}/undo`, {});
  }

  resetGame(id: string): Observable<GameState> {
    return this.http.post<GameState>(`${API_BASE}/games/${id}/reset`, {});
  }

  getScoreboard(): Observable<Scoreboard> {
    return this.http.get<Scoreboard>(`${API_BASE}/scoreboard`);
  }

  resetScoreboard(): Observable<Scoreboard> {
    return this.http.post<Scoreboard>(`${API_BASE}/scoreboard/reset`, {});
  }
}
