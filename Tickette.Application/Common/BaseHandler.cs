using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Tickette.Application.Common;

public abstract class BaseHandler<T>
{
    private readonly ILogger<T> _logger;

    protected BaseHandler(ILogger<T> logger)
    {
        _logger = logger;
    }

    protected async Task<TResult> ExecuteWithErrorHandlingAsync<TResult>(Func<Task<TResult>> operation, string operationName)
    {
        try
        {
            return await operation();
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error during {OperationName}.", operationName);
            throw new Exception("A database error occurred. Please try again later.", dbEx);
        }
        catch (ValidationException valEx)
        {
            _logger.LogWarning(valEx, "Validation error during {OperationName}: {Message}", operationName, valEx.Message);
            throw new Exception("Invalid input data. Please check your details.", valEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during {OperationName}.", operationName);
            throw new Exception("An unexpected error occurred. Please try again later.", ex);
        }
    }
}
