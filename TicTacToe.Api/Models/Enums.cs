namespace TicTacToe.Api.Models;

public enum Player
{
    X = 0,
    O = 1
}

public enum GameMode
{
    TwoPlayer = 0,
    Computer = 1
}

public enum GameStatus
{
    InProgress = 0,
    Won = 1,
    Draw = 2
}

public enum Difficulty
{
    Easy = 0,
    Medium = 1,
    Hard = 2
}
