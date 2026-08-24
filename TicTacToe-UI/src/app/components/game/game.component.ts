import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval, Subscription } from 'rxjs';
import { GameService } from '../../services/game.service';
import {
  GameStateResponse,
  ScoreboardResponse,
  GameMode,
  Difficulty
} from '../../models/game.models';

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './game.component.html',
  styleUrl: './game.component.css'
})
export class GameComponent implements OnInit, OnDestroy {
  static readonly MOVE_TIME_LIMIT = 10;

  gameState = signal<GameStateResponse | null>(null);
  scoreboard = signal<ScoreboardResponse>({ xWins: 0, oWins: 0, draws: 0 });
  loading = signal(false);
  errorMessage = signal('');
  selectedMode = signal<GameMode>(GameMode.TwoPlayer);
  selectedDifficulty = signal<Difficulty>(Difficulty.Medium);
  showDifficulty = signal(false);
  timeLeft = signal<number>(GameComponent.MOVE_TIME_LIMIT);

  private timerSub?: Subscription;

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    this.loadScoreboard();
  }

  ngOnDestroy(): void {
    this.stopMoveTimer();
  }

  get isGameActive(): boolean {
    const state = this.gameState();
    return state !== null && state.status === 'InProgress';
  }

  get isGameOver(): boolean {
    const state = this.gameState();
    return state !== null && state.status !== 'InProgress';
  }

  get isComputerMode(): boolean {
    return this.gameState()?.gameMode === GameMode.Computer.toString();
  }

  get isWinningCell(): (row: number, col: number) => boolean {
    return (row: number, col: number) => {
      const state = this.gameState();
      if (!state?.winningCells) return false;
      return state.winningCells.some(c => c[0] === row && c[1] === col);
    };
  }

  chooseMode(mode: GameMode): void {
    if (mode === GameMode.Computer) {
      this.showDifficulty.set(true);
      return;
    }
    this.showDifficulty.set(false);
    this.startGame(mode);
  }

  startGame(mode: GameMode, difficulty?: Difficulty): void {
    if (difficulty !== undefined) {
      this.selectedDifficulty.set(difficulty);
    }
    this.loading.set(true);
    this.errorMessage.set('');
    this.gameService.createGame(mode, difficulty).subscribe({
      next: (game) => {
        this.gameState.set(game);
        this.selectedMode.set(mode);
        this.showDifficulty.set(false);
        this.loading.set(false);
        this.startMoveTimer();
      },
      error: (err) => {
        this.errorMessage.set('Failed to create game. Is the backend running?');
        this.loading.set(false);
        console.error(err);
      }
    });
  }

  makeMove(row: number, col: number): void {
    const state = this.gameState();
    if (!state || !this.isGameActive) return;
    if (state.board[row][col] !== '') return;
    if (this.loading()) return;

    this.stopMoveTimer();
    this.loading.set(true);
    this.errorMessage.set('');
    const player = state.currentPlayer === 'X' ? 0 : 1;

    this.gameService.makeMove(state.id, player, row, col).subscribe({
      next: (game) => {
        this.gameState.set(game);
        this.loading.set(false);
        if (game.status !== 'InProgress') {
          this.loadScoreboard();
        } else {
          this.startMoveTimer();
        }
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to make move.');
        this.loading.set(false);
        this.startMoveTimer();
      }
    });
  }

  undoMove(): void {
    const state = this.gameState();
    if (!state || !state.canUndo || this.loading()) return;

    this.stopMoveTimer();
    this.loading.set(true);
    this.errorMessage.set('');

    this.gameService.undoMove(state.id).subscribe({
      next: (game) => {
        this.gameState.set(game);
        this.loading.set(false);
        this.startMoveTimer();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to undo move.');
        this.loading.set(false);
        this.startMoveTimer();
      }
    });
  }

  resetGame(): void {
    const state = this.gameState();
    if (!state || this.loading()) return;

    this.stopMoveTimer();
    this.loading.set(true);
    this.errorMessage.set('');

    this.gameService.resetGame(state.id).subscribe({
      next: (game) => {
        this.gameState.set(game);
        this.loading.set(false);
        this.startMoveTimer();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to reset game.');
        this.loading.set(false);
        this.startMoveTimer();
      }
    });
  }

  resetScoreboard(): void {
    if (this.loading()) return;

    this.loading.set(true);
    this.errorMessage.set('');

    this.gameService.resetScoreboard().subscribe({
      next: (scoreboard) => {
        this.scoreboard.set(scoreboard);
        this.loading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to reset scoreboard.');
        this.loading.set(false);
      }
    });
  }

  private loadScoreboard(): void {
    this.gameService.getScoreboard().subscribe({
      next: (scoreboard) => {
        this.scoreboard.set(scoreboard);
      },
      error: (err) => {
        console.error('Failed to load scoreboard', err);
      }
    });
  }

  private startMoveTimer(): void {
    this.stopMoveTimer();
    const state = this.gameState();
    if (!state || state.status !== 'InProgress') return;

    this.timeLeft.set(GameComponent.MOVE_TIME_LIMIT);
    this.timerSub = interval(1000).subscribe(() => {
      const remaining = this.timeLeft() - 1;
      if (remaining <= 0) {
        this.stopMoveTimer();
        this.handleMoveTimeout();
      } else {
        this.timeLeft.set(remaining);
      }
    });
  }

  private stopMoveTimer(): void {
    this.timerSub?.unsubscribe();
    this.timerSub = undefined;
  }

  private handleMoveTimeout(): void {
    const state = this.gameState();
    if (!state || state.status !== 'InProgress') return;

    const emptyCells: [number, number][] = [];
    for (let row = 0; row < 3; row++) {
      for (let col = 0; col < 3; col++) {
        if (state.board[row][col] === '') {
          emptyCells.push([row, col]);
        }
      }
    }

    if (emptyCells.length === 0) return;

    const [row, col] = emptyCells[Math.floor(Math.random() * emptyCells.length)];
    this.errorMessage.set(`Time's up! A random move was played for ${state.currentPlayer}.`);
    this.makeMove(row, col);
  }

  getWinnerDisplay(): string {
    const winner = this.gameState()?.winner;
    if (!winner) return '';
    return winner === 'X' ? '✕' : '○';
  }

  getModeTag(): string {
    const state = this.gameState();
    if (!state) return '';
    if (state.gameMode !== 'Computer') return '👥 Two Player';
    const label = state.difficulty ?? Difficulty[this.selectedDifficulty()];
    return `🤖 vs Computer (${label})`;
  }

  getStatusMessage(): string {
    const state = this.gameState();
    if (!state) return '';
    if (state.status === 'Won') {
      return `${state.winner} wins!`;
    }
    if (state.status === 'Draw') {
      return "It's a draw!";
    }
    return `${state.currentPlayer}'s turn`;
  }

  getCellDisplay(row: number, col: number): string {
    const state = this.gameState();
    if (!state) return '';
    const cell = state.board[row][col];
    if (cell === 'X') return '✕';
    if (cell === 'O') return '○';
    return '';
  }

  isCellOccupied(row: number, col: number): boolean {
    const state = this.gameState();
    if (!state) return false;
    return state.board[row][col] !== '';
  }
}