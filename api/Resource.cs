namespace api;

public abstract class Resource<T>
{
    private Resource() { }

    public sealed class Success(T data) : Resource<T>
    {
        public T Data { get; } = data;
    }

    public sealed class Error(string message) : Resource<T>
    {
        public string ErrorMessage { get; } = message;
    }
    
    public sealed class ServerError(string error) : Resource<T>
    {
        public AppError Error { get; } = ParseError(error);

        private static AppError ParseError(string error) => error switch
        {
            "User not found" => AppError.UserNotFound,
            "Invalid parameters" => AppError.InvalidParameters,
            "Failed to fetch user" => AppError.UserSuccessFailed,
            "No segments was found" => AppError.NoSegmentsWasFound,
            "Server is overloading" => AppError.ServerOverloading,
            "Connection lost" => AppError.ConnectionLost,
            _ => AppError.Unknown
        };
    }
}

public enum AppError
{
    UserNotFound,
    InvalidParameters,
    UserSuccessFailed,
    NoSegmentsWasFound,
    ServerOverloading,
    ConnectionLost,
    Unknown
}