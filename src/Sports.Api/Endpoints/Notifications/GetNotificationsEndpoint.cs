using FastEndpoints;
using System.Security.Claims;
using Sports.Application.DTOs.Notifications;
using Sports.Application.DTOs;
using Sports.Application.Interfaces;

namespace Sports.Api.Endpoints.Notifications;

public class GetNotificationsEndpoint : EndpointWithoutRequest<PagedResponse<NotificationDto>>
{
    private readonly INotificationService _notificationService;

    public GetNotificationsEndpoint(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public override void Configure()
    {
        Get("/api/fast/notifications");
        Description(d => d.WithName("GetNotifications"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        // Read query parameters directly from HttpContext
        var unreadOnlyStr = HttpContext.Request.Query["unreadOnly"].FirstOrDefault();
        var pageNumberStr = HttpContext.Request.Query["pageNumber"].FirstOrDefault();
        var pageSizeStr = HttpContext.Request.Query["pageSize"].FirstOrDefault();

        bool unreadOnly = bool.TryParse(unreadOnlyStr, out var uo) && uo;
        int pageNumber = int.TryParse(pageNumberStr, out var pn) && pn > 0 ? pn : 1;
        int pageSize = int.TryParse(pageSizeStr, out var ps) && ps > 0 ? ps : 10;

        var allNotifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);

        var list = allNotifications.ToList();
        var pagedItems = list
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var response = new PagedResponse<NotificationDto>
        {
            Items = pagedItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = list.Count
        };

        await SendOkAsync(response, ct);
    }
}