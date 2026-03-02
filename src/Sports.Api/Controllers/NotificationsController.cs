using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs.Notifications;
using Sports.Application.Interfaces;
using Sports.Domain.Constants;
using System.Security.Claims;

namespace Sports.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]  
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService NotificationService;

    public NotificationsController(INotificationService notificationService)
    {
        NotificationService = notificationService;
    }

    /// <summary>
    /// Get all notifications for the current user with optional pagination
    /// </summary>
    /// <param name="unreadOnly"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMyNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var notifications = await NotificationService.GetUserNotificationsAsync(userId, unreadOnly);
        
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            var notificationsList = notifications.ToList();
            var pagedNotifications = notificationsList
                .Skip((pageNumber.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
            
            return Ok(new
            {
                Items = pagedNotifications,
                TotalCount = notificationsList.Count,
                PageNumber = pageNumber.Value,
                PageSize = pageSize.Value,
                TotalPages = (int)Math.Ceiling(notificationsList.Count / (double)pageSize.Value),
                HasPrevious = pageNumber.Value > 1,
                HasNext = pageNumber.Value < (int)Math.Ceiling(notificationsList.Count / (double)pageSize.Value)
            });
        }
        
        return Ok(notifications);
    }

    /// <summary>
    /// Get unread notification count
    /// </summary>
    /// <returns></returns>
    [HttpGet("unread-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var count = await NotificationService.GetUnreadCountAsync(userId);
        return Ok(new { count });
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await NotificationService.MarkAsReadAsync(id);
        if (!result) return NotFound();

        return Ok(new { message = MessageStrings.NotificationMarkedAsRead });
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    /// <returns></returns>
    [HttpPost("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await NotificationService.MarkAllAsReadAsync(userId);
        return Ok(new { message = MessageStrings.AllNotificationsMarkedAsRead });
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await NotificationService.DeleteNotificationAsync(id, userId);
        if (!result) return NotFound();

        return Ok(new { message = MessageStrings.NotificationDeleted });
    }

    /// <summary>
    /// Delete all notifications
    /// </summary>
    /// <returns></returns>
    [HttpDelete("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAllNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await NotificationService.DeleteAllNotificationsAsync(userId);
        return Ok(new { message = MessageStrings.AllNotificationsDeleted });
    }
}