namespace Sports.Application.DTOs.Games;

public class JoinRequestDto
{
    public int RequestId { get; set; }
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? ResponseMessage { get; set; }
}

public class CreateJoinRequestDto
{
    public string? Message { get; set; }
}

public class RespondToJoinRequestDto
{
    public bool Approve { get; set; }
    public string? ResponseMessage { get; set; }
}