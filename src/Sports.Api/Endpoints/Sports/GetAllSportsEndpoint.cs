namespace Sports.Api.Endpoints.Sports;

using FastEndpoints;
using global::Sports.Application.DTOs.Games;
using global::Sports.Application.Interfaces;

public class GetAllSportsEndpoint : EndpointWithoutRequest<IEnumerable<SportDto>>
{
    private readonly IAdminService _adminService;
    
    public GetAllSportsEndpoint(IAdminService adminService)
    {
        _adminService = adminService;
    }
    
    public override void Configure()
    {
        Get("/api/fast/sports");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var sports = await _adminService.GetAllSportsAsync();
        await SendOkAsync(sports, ct);
    }
}