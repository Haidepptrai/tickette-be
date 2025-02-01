namespace Tickette.Application.Wrappers;

public static class ResponseHandler
{
    public static ResponseDto<T> SuccessResponse<T>(T data, string message = "Operation completed successfully.", int statusCode = 200)
    {
        return new ResponseDto<T>(message, data, statusCode, true);
    }

    public static ResponseDto<T> ErrorResponse<T>(T? data, string message = "Operation failed.", int statusCode = 400)
    {
        return new ResponseDto<T>(message, data, statusCode, false);
    }

    public static ResponseDto<IEnumerable<T>> PaginatedResponse<T>(
        IEnumerable<T> data,
        PaginationMeta meta,
        string message = "Operation completed successfully.",
        int statusCode = 200)
    {
        return new ResponseDto<IEnumerable<T>>(message, data, statusCode, true, meta);
    }
}

public class ResponseDto<T>
{
    public string Message { get; set; }
    public T? Data { get; set; }
    public int StatusCode { get; set; }
    public bool Success { get; set; }
    public PaginationMeta? Meta { get; set; } // Optional pagination metadata

    public ResponseDto(string message, T? data, int statusCode, bool success, PaginationMeta? meta = null)
    {
        Message = message;
        Data = data;
        StatusCode = statusCode;
        Success = success;
        Meta = meta;
    }
}

public class PaginationMeta
{
    public int Page { get; set; }

    public int PerPage { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }

    public PaginationMeta(int page, int perPage, int totalItems, int totalPages)
    {
        Page = page;
        PerPage = perPage;
        TotalItems = totalItems;
        TotalPages = totalPages;
    }
}
