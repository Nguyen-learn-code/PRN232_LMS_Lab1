namespace PRN232.LMS.API.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Errors { get; set; }
    public PaginationMetadata? Pagination { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Request processed successfully", PaginationMetadata? pagination = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = null,
            Pagination = pagination
        };
    }

    public static ApiResponse<T?> Fail(string message, object? errors = null)
    {
        return new ApiResponse<T?>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors,
            Pagination = null
        };
    }
}

public class PaginationMetadata
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
