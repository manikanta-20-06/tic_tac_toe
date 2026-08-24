namespace TicTacToe.Api.DTOs;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }

    public ErrorResponse(string message)
    {
        Message = message;
    }

    public ErrorResponse(string message, string details)
    {
        Message = message;
        Details = details;
    }
}
