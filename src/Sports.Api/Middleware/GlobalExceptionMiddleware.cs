using System.Net;
using System.Text.Json;
using Sports.Application.Exceptions;
using Sports.Domain.Constants;

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
            Logger.LogError(ex, MessageStrings.UnhandledExceptionLog, ex.Message);
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
                MessageStrings.ResourceNotFoundTitle, 
                nf.Message, 
                (Dictionary<string, string[]>?)null),

            ValidationException ve => (
                HttpStatusCode.BadRequest, 
                MessageStrings.ValidationErrorTitle, 
                ve.Message, 
                ve.Errors),

            UnauthorizedException ue => (
                HttpStatusCode.Unauthorized, 
                MessageStrings.UnauthorizedTitle, 
                ue.Message, 
                null),

            GameFullException => (
                HttpStatusCode.BadRequest, 
                MessageStrings.GameFullTitle, 
                MessageStrings.GameFullMessage, 
                null),

            AlreadyJoinedException => (
                HttpStatusCode.BadRequest, 
                MessageStrings.AlreadyJoinedTitle, 
                MessageStrings.AlreadyJoinedMessage, 
                null),

            GameCancelledException => (
                HttpStatusCode.BadRequest, 
                MessageStrings.GameCancelledTitle, 
                MessageStrings.GameCancelledMessage, 
                null),

            PastGameException => (
                HttpStatusCode.BadRequest, 
                MessageStrings.PastGameTitle, 
                MessageStrings.PastGameMessage, 
                null),

            DuplicateException de => (
                HttpStatusCode.Conflict, 
                MessageStrings.DuplicateEntryTitle, 
                de.Message, 
                null),

            AccountLockedException ale => (
                HttpStatusCode.Locked, 
                MessageStrings.AccountLockedTitle, 
                ale.Message, 
                null),

            EntityInUseException eiu => (
                HttpStatusCode.Conflict, 
                MessageStrings.EntityInUseTitle, 
                eiu.Message, 
                null),

            _ => (
                HttpStatusCode.InternalServerError, 
                MessageStrings.InternalServerErrorTitle, 
                Env.IsDevelopment() ? ex.Message : MessageStrings.GenericUnexpectedError, 
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
