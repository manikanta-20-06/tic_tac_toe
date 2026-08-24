import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { GameService } from './game.service';
import { GameMode } from '../models/game.models';

describe('GameService', () => {
  let service: GameService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        GameService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(GameService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should create a game via POST', () => {
    const mockResponse = {
      id: 'test-id',
      board: [['', '', ''], ['', '', ''], ['', '', '']],
      currentPlayer: 'X',
      gameMode: 'TwoPlayer',
      status: 'InProgress',
      winner: null,
      winningCells: [],
      moveHistory: [],
      createdAt: new Date().toISOString(),
      canUndo: false
    };

    service.createGame(GameMode.TwoPlayer).subscribe(result => {
      expect(result.id).toBe('test-id');
      expect(result.status).toBe('InProgress');
    });

    const req = httpMock.expectOne('/api/games');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ gameMode: GameMode.TwoPlayer });
    req.flush(mockResponse);
  });

  it('should get game state via GET', () => {
    const gameId = 'test-id';
    const mockResponse = {
      id: gameId,
      board: [['', '', ''], ['', '', ''], ['', '', '']],
      currentPlayer: 'X',
      gameMode: 'TwoPlayer',
      status: 'InProgress',
      winner: null,
      winningCells: [],
      moveHistory: [],
      createdAt: new Date().toISOString(),
      canUndo: false
    };

    service.getGame(gameId).subscribe(result => {
      expect(result.id).toBe(gameId);
    });

    const req = httpMock.expectOne(`/api/games/${gameId}`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should make a move via POST', () => {
    const gameId = 'test-id';
    const mockResponse = {
      id: gameId,
      board: [['X', '', ''], ['', '', ''], ['', '', '']],
      currentPlayer: 'O',
      gameMode: 'TwoPlayer',
      status: 'InProgress',
      winner: null,
      winningCells: [],
      moveHistory: [{ player: 'X', row: 0, column: 0, moveNumber: 1, timestamp: new Date().toISOString() }],
      createdAt: new Date().toISOString(),
      canUndo: true
    };

    service.makeMove(gameId, 0, 0, 0).subscribe(result => {
      expect(result.currentPlayer).toBe('O');
    });

    const req = httpMock.expectOne(`/api/games/${gameId}/moves`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ player: 0, row: 0, column: 0 });
    req.flush(mockResponse);
  });

  it('should undo move via POST', () => {
    const gameId = 'test-id';
    const mockResponse = {
      id: gameId,
      board: [['', '', ''], ['', '', ''], ['', '', '']],
      currentPlayer: 'X',
      gameMode: 'TwoPlayer',
      status: 'InProgress',
      winner: null,
      winningCells: [],
      moveHistory: [],
      createdAt: new Date().toISOString(),
      canUndo: false
    };

    service.undoMove(gameId).subscribe(result => {
      expect(result.moveHistory.length).toBe(0);
    });

    const req = httpMock.expectOne(`/api/games/${gameId}/undo`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('should reset game via POST', () => {
    const gameId = 'test-id';
    const mockResponse = {
      id: gameId,
      board: [['', '', ''], ['', '', ''], ['', '', '']],
      currentPlayer: 'X',
      gameMode: 'TwoPlayer',
      status: 'InProgress',
      winner: null,
      winningCells: [],
      moveHistory: [],
      createdAt: new Date().toISOString(),
      canUndo: false
    };

    service.resetGame(gameId).subscribe(result => {
      expect(result.status).toBe('InProgress');
    });

    const req = httpMock.expectOne(`/api/games/${gameId}/reset`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('should get scoreboard via GET', () => {
    const mockResponse = { xWins: 1, oWins: 2, draws: 3 };

    service.getScoreboard().subscribe(result => {
      expect(result.xWins).toBe(1);
      expect(result.oWins).toBe(2);
      expect(result.draws).toBe(3);
    });

    const req = httpMock.expectOne('/api/scoreboard');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should reset scoreboard via POST', () => {
    const mockResponse = { xWins: 0, oWins: 0, draws: 0 };

    service.resetScoreboard().subscribe(result => {
      expect(result.xWins).toBe(0);
    });

    const req = httpMock.expectOne('/api/scoreboard/reset');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });
});
