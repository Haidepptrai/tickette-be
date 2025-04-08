namespace Tickette.Application.Wrappers;

public static class ResponseHandler
{
    public static ResponseDto<T> SuccessResponse<T>(T data, string message = "Operation completed successfully.")
    {
        return new ResponseDto<T>(message, data);
    }

    public static ResponseDto<T> ErrorResponse<T>(T? data, string message = "Operation failed.")
    {
        return new ResponseDto<T>(message, data);
    }

    public static ResponseDto<IEnumerable<T>> PaginatedResponse<T>(
        IEnumerable<T> data,
        PaginationMeta meta,
        string message = "Operation completed successfully.",
        int statusCode = 200)
    {
        return new ResponseDto<IEnumerable<T>>(message, data, meta);
    }
}

public class ResponseDto<T>
{
    public string Message { get; set; }
    public T? Data { get; set; }
    public PaginationMeta? Meta { get; set; } // Optional pagination metadata

    public ResponseDto(string message, T? data, PaginationMeta? meta = null)
    {
        Message = message;
        Data = data;
        Meta = meta;
    }
}

public class PaginationMeta
{
    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }

    public PaginationMeta(int pageNumber, int pageSize, int totalItems, int totalPages)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = totalPages;
    }
}