using AutoMapper;
using Sports.Domain.Entities;
using Sports.Application.DTOs.Games;
using Sports.Application.DTOs.Admin;
using Sports.Application.DTOs.Notifications;
using Sports.Application.DTOs.Chat;

namespace Sports.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ========== GAME MAPPINGS ==========
        CreateMap<Game, GameDto>()
            .ForMember(d => d.SportName, o => o.MapFrom(s => s.Sport.Name))
            .ForMember(d => d.SportIcon, o => o.MapFrom(s => s.Sport.IconUrl ?? "🏅"))
            .ForMember(d => d.VenueName, o => o.MapFrom(s => s.Venue.Name))
            .ForMember(d => d.PlayerCount, o => o.MapFrom(s => s.Participants.Count))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.HostName, o => o.Ignore()) // Set manually in service
            .ForMember(d => d.PendingRequestsCount, o => o.MapFrom(s => 
                s.JoinRequests.Count(r => r.Status == Domain.Enums.RequestStatus.Pending)));

        CreateMap<Game, GameDetailDto>()
            .IncludeBase<Game, GameDto>()
            .ForMember(d => d.Participants, o => o.MapFrom(s => s.Participants))
            .ForMember(d => d.PendingRequests, o => o.Ignore()) // Set manually
            .ForMember(d => d.CurrentUserHasRequest, o => o.Ignore()); // Set manually

        CreateMap<CreateGameDto, Game>()
            .ForMember(d => d.GameId, o => o.Ignore())
            .ForMember(d => d.Sport, o => o.Ignore())
            .ForMember(d => d.Venue, o => o.Ignore())
            .ForMember(d => d.Participants, o => o.Ignore())
            .ForMember(d => d.JoinRequests, o => o.Ignore())
            .ForMember(d => d.HostUserId, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(_ => Domain.Enums.GameStatus.Open));

        // ========== PARTICIPANT MAPPINGS ==========
        CreateMap<GameParticipant, ParticipantDto>()
            .ForMember(d => d.UserName, o => o.Ignore()) // Set manually
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.IsHost, o => o.Ignore()); // Set manually

        // ========== SPORT MAPPINGS ==========
        CreateMap<Sport, SportDto>();
        
        CreateMap<CreateSportDto, Sport>()
            .ForMember(d => d.SportId, o => o.Ignore());

        // ========== VENUE MAPPINGS ==========
        CreateMap<Venue, VenueDto>();
        
        CreateMap<CreateVenueDto, Venue>()
            .ForMember(d => d.VenueId, o => o.Ignore());

        // ========== EMPLOYEE MAPPINGS ==========
        CreateMap<EmployeeDirectory, EmployeeDto>();
        
        CreateMap<CreateEmployeeDto, EmployeeDirectory>()
            .ForMember(d => d.EmployeeId, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true));

        // ========== NOTIFICATION MAPPINGS ==========
        CreateMap<Notification, NotificationDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        // ========== JOIN REQUEST MAPPINGS ==========
        CreateMap<JoinRequest, JoinRequestDto>()
            .ForMember(d => d.GameTitle, o => o.MapFrom(s => s.Game.Title))
            .ForMember(d => d.UserName, o => o.Ignore()) // Set manually
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        // ========== CHAT MAPPINGS ==========
        CreateMap<ChatMessage, ChatMessageDto>()
            .ForMember(d => d.SenderName, o => o.Ignore()) // Set manually
            .ForMember(d => d.IsCurrentUser, o => o.Ignore()); // Set manually
    }
}
