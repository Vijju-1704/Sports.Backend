using System.Net;
using System.Text.Json;
using Sports.Application.Exceptions;

namespace Sports.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate Next;
    private readonly ILogger<GlobalExceptionMiddleware> Logger;
    private readonly IHostEnvironment Env;

    public GlobalExceptionMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionMiddleware> logger, 
        IHostEnvironment env)
    {
        Next = next;
        Logger = logger;
        Env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await Next(context);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, title, message, errors) = ex switch
        {
            NotFoundException nf => (
                HttpStatusCode.NotFound, 
                "Resource Not Found", 
                nf.Message, 
                (Dictionary<string, string[]>?)null),

            ValidationException ve => (
                HttpStatusCode.BadRequest, 
                "Validation Error", 
                ve.Message, 
                ve.Errors),

            UnauthorizedException ue => (
                HttpStatusCode.Unauthorized, 
                "Unauthorized", 
                ue.Message, 
                null),

            GameFullException => (
                HttpStatusCode.BadRequest, 
                "Game Full", 
                "This game is already full. Please try another game.", 
                null),

            AlreadyJoinedException => (
                HttpStatusCode.BadRequest, 
                "Already Joined", 
                "You have already joined this game.", 
                null),

            GameCancelledException => (
                HttpStatusCode.BadRequest, 
                "Game Cancelled", 
                "This game has been cancelled.", 
                null),

            PastGameException => (
                HttpStatusCode.BadRequest, 
                "Past Game", 
                "Cannot perform this action on a game that has already started.", 
                null),

            DuplicateException de => (
                HttpStatusCode.Conflict, 
                "Duplicate Entry", 
                de.Message, 
                null),

            AccountLockedException ale => (
                HttpStatusCode.Locked, 
                "Account Locked", 
                ale.Message, 
                null),

            EntityInUseException eiu => (
                HttpStatusCode.Conflict, 
                "Entity In Use", 
                eiu.Message, 
                null),

            _ => (
                HttpStatusCode.InternalServerError, 
                "Internal Server Error", 
                Env.IsDevelopment() ? ex.Message : "An unexpected error occurred. Please try again later.", 
                null)
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            statusCode = (int)statusCode,
            title,
            message,
            errors,
            timestamp = DateTime.UtcNow
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
