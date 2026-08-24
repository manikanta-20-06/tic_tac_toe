import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateGameRequest,
  GameStateResponse,
  MakeMoveRequest,
  ScoreboardResponse,
  GameMode,
  Difficulty
} from '../models/game.models';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private readonly apiUrl = '/api';

  constructor(private http: HttpClient) {}

  createGame(gameMode: GameMode = GameMode.TwoPlayer, difficulty?: Difficulty): Observable<GameStateResponse> {
    const request: CreateGameRequest = { gameMode, difficulty };
    return this.http.post<GameStateResponse>(`${this.apiUrl}/games`, request);
  }

  getGame(id: string): Observable<GameStateResponse> {
    return this.http.get<GameStateResponse>(`${this.apiUrl}/games/${id}`);
  }

  makeMove(gameId: string, player: number, row: number, column: number): Observable<GameStateResponse> {
    const request: MakeMoveRequest = { player, row, column };
    return this.http.post<GameStateResponse>(`${this.apiUrl}/games/${gameId}/moves`, request);
  }

  undoMove(gameId: string): Observable<GameStateResponse> {
    return this.http.post<GameStateResponse>(`${this.apiUrl}/games/${gameId}/undo`, {});
  }

  resetGame(gameId: string): Observable<GameStateResponse> {
    return this.http.post<GameStateResponse>(`${this.apiUrl}/games/${gameId}/reset`, {});
  }

  getScoreboard(): Observable<ScoreboardResponse> {
    return this.http.get<ScoreboardResponse>(`${this.apiUrl}/scoreboard`);
  }

  resetScoreboard(): Observable<ScoreboardResponse> {
    return this.http.post<ScoreboardResponse>(`${this.apiUrl}/scoreboard/reset`, {});
  }
}
