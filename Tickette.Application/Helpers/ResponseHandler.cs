namespace Tickette.Application.Helpers;

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
}

public class ResponseDto<T>
{
    public string Message { get; set; }
    public T? Data { get; set; }
    public int StatusCode { get; set; }
    public bool Success { get; set; }

    public ResponseDto(string message, T? data, int statusCode, bool success)
    {
        this.Message = message;
        this.Data = data;
        this.StatusCode = statusCode;
        this.Success = success;
    }
}