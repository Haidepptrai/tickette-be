using System.Text.Json;
using Tickette.Application.Exceptions;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;
using Tickette.Domain.Common.Exceptions;

namespace Tickette.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InsufficientTicketsException ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, 409, ex.Message);
        }
        catch (NotFoundTicketReservationException ex)
        {
            await HandleExceptionAsync(context, 404, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(context, 404, ex.Message);
        }
        catch (InvalidQuantityException ex)
        {
            await HandleExceptionAsync(context, 400, ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No service for type"))
        {
            _logger.LogError("A required service was not registered: {Message}", ex.Message);
            await HandleExceptionAsync(context, 500, "Internal configuration error");
        }
        catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(context, 400, ex.Message);
        }
        catch (UserAlreadyExistException ex)
        {
            await HandleExceptionAsync(context, 409, ex.Message);
        }
        catch (SeatOrderedException ex)
        {
            await HandleExceptionAsync(context, 409, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, 500, "An error has occurred");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ResponseHandler.ErrorResponse(Unit.Value, message, statusCode);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, jsonOptions);
        return context.Response.WriteAsync(json);
    }
}