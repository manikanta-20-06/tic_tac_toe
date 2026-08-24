export enum Player {
  X = 0,
  O = 1
}

export enum GameMode {
  TwoPlayer = 0,
  Computer = 1
}

export enum Difficulty {
  Easy = 0,
  Medium = 1,
  Hard = 2
}

export enum GameStatus {
  InProgress = 0,
  Won = 1,
  Draw = 2
}

export interface CreateGameRequest {
  gameMode: GameMode;
  difficulty?: Difficulty;
}

export interface MakeMoveRequest {
  player: Player;
  row: number;
  column: number;
}

export interface MoveResponse {
  player: string;
  row: number;
  column: number;
  moveNumber: number;
  timestamp: string;
}

export interface GameStateResponse {
  id: string;
  board: string[][];
  currentPlayer: string;
  gameMode: string;
  difficulty?: string;
  status: string;
  winner: string | null;
  winningCells: number[][];
  moveHistory: MoveResponse[];
  createdAt: string;
  canUndo: boolean;
}

export interface ScoreboardResponse {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface ErrorResponse {
  message: string;
  details?: string;
}
