using System.ComponentModel.DataAnnotations;
namespace Sports.Application.DTOs.Games;

public class GameDto
{
    public int GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SportId { get; set; }
    public string SportName { get; set; } = string.Empty;
    public string SportIcon { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public int VenueId { get; set; } 
    public DateTime DateTime { get; set; }
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty; 
    public int PlayerCount { get; set; }
    public int MaxPlayers { get; set; }
    public int MinPlayers { get; set; } 
    public decimal? CostPerPerson { get; set; }
    public string HostName { get; set; } = string.Empty;
    public string HostUserId { get; set; } = string.Empty;
    public bool RequireApproval { get; set; }
    public int PendingRequestsCount { get; set; }
}

public class CreateGameDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public int SportId { get; set; }

    [Required]
    public int VenueId { get; set; }

    [Required]
    public DateTime DateTime { get; set; }

    [Range(30, 300)]
    public int DurationMinutes { get; set; } = 60;

    [Range(2, 50)]
    public int MinPlayers { get; set; }

    [Range(2, 50)]
    public int MaxPlayers { get; set; }

    public decimal? CostPerPerson { get; set; }
    public string? EquipmentNeeded { get; set; }
    public bool RequireApproval { get; set; } = false;
}

public class GameDetailDto : GameDto
{
    public string? EquipmentNeeded { get; set; }
    public string? Description { get; set; }
    public List<ParticipantDto> Participants { get; set; } = new();
    public List<JoinRequestDto> PendingRequests { get; set; } = new();
    public bool CurrentUserHasRequest { get; set; }
}

public class ParticipantDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public bool IsHost { get; set; }
}



//Adding more DTOssssssssssss

public class SportDto
{
    public int SportId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
}

public class VenueDto
{
    public int VenueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? MapsUrl { get; set; }
    public string? Facilities { get; set; }
}

public class CreateSportDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
}

public class CreateVenueDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? MapsUrl { get; set; }
    public string? Facilities { get; set; }
}


public class UpdateGameStatusDto
{
    public string Status { get; set; } = string.Empty; // "InProgress", "Completed", "Cancelled"
}