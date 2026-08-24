import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { GameComponent } from './game.component';

describe('GameComponent', () => {
  let component: GameComponent;
  let fixture: ComponentFixture<GameComponent>;
  let httpMock: HttpTestingController;

  const mockGameState = {
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

  const mockScoreboard = { xWins: 0, oWins: 0, draws: 0 };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(GameComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);

    // Trigger ngOnInit
    fixture.detectChanges();

    // Flush the initial scoreboard request
    const req = httpMock.expectOne('/api/scoreboard');
    req.flush(mockScoreboard);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display mode selector initially', () => {
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Select Game Mode');
    expect(compiled.textContent).toContain('Two Player');
    expect(compiled.textContent).toContain('vs Computer');
  });

  it('should start a game when mode button clicked', () => {
    component.startGame(0);

    const req = httpMock.expectOne('/api/games');
    req.flush(mockGameState);

    expect(component.gameState()).toBeTruthy();
    expect(component.gameState()!.id).toBe('test-id');
  });

  it('should show error when backend is not running', () => {
    component.startGame(0);

    const req = httpMock.expectOne('/api/games');
    req.error(new ProgressEvent('error'));

    expect(component.errorMessage()).toContain('Failed to create game');
  });

  it('should display correct status for game states', () => {
    component.gameState.set({ ...mockGameState, status: 'Won', winner: 'X' });
    expect(component.getStatusMessage()).toBe('X wins!');

    component.gameState.set({ ...mockGameState, status: 'Draw', winner: null });
    expect(component.getStatusMessage()).toBe("It's a draw!");

    component.gameState.set({ ...mockGameState, status: 'InProgress' });
    expect(component.getStatusMessage()).toBe("X's turn");
  });

  it('should return correct cell display', () => {
    component.gameState.set({
      ...mockGameState,
      board: [['X', 'O', ''], ['O', '', 'X'], ['', 'X', '']]
    });

    expect(component.getCellDisplay(0, 0)).toBe('✕');
    expect(component.getCellDisplay(0, 1)).toBe('○');
    expect(component.getCellDisplay(0, 2)).toBe('');
  });

  it('should detect occupied cells', () => {
    component.gameState.set({
      ...mockGameState,
      board: [['X', '', ''], ['', '', ''], ['', '', '']]
    });

    expect(component.isCellOccupied(0, 0)).toBe(true);
    expect(component.isCellOccupied(0, 1)).toBe(false);
  });

  it('should disable board when game is over', () => {
    component.gameState.set({ ...mockGameState, status: 'Won', winner: 'X' });
    expect(component.isGameActive).toBe(false);
    expect(component.isGameOver).toBe(true);
  });

  it('should disable undo when no moves', () => {
    component.gameState.set({ ...mockGameState, canUndo: false });
    expect(component.gameState()!.canUndo).toBe(false);
  });

  it('should show winning cells', () => {
    component.gameState.set({
      ...mockGameState,
      winningCells: [[0, 0], [0, 1], [0, 2]]
    });

    expect(component.isWinningCell(0, 0)).toBe(true);
    expect(component.isWinningCell(1, 1)).toBe(false);
  });

  it('should reset game', () => {
    component.gameState.set({ ...mockGameState, id: 'test-id' });
    component.resetGame();

    const req = httpMock.expectOne('/api/games/test-id/reset');
    req.flush({ ...mockGameState, moveHistory: [] });

    expect(component.gameState()!.moveHistory.length).toBe(0);
  });

  it('should return to mode selector on new game', () => {
    component.gameState.set(mockGameState);
    component.gameState.set(null);
    expect(component.gameState()).toBeNull();
  });
});