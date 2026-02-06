namespace FootballTournament.API.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    // Primary methods (short names)
    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = null
        };
    }

    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = null,
            Data = data,
            Errors = null
        };
    }

    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors
        };
    }

    public static ApiResponse<T> Fail(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = null
        };
    }

    // Aliases for backward compatibility
    public static ApiResponse<T> SuccessResponse(T data, string? message = null) => Ok(data, message);
    public static ApiResponse<T> FailResponse(string message, List<string>? errors = null) => Fail(message, errors);
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null) => Fail(message, errors);
}
