namespace GoogleAdk.Core.Abstractions.Errors;

public enum ToolErrorType
{
    Unknown,
    Validation,
    Execution,
    Timeout
}

public sealed class ToolExecutionError : Exception
{
    public ToolErrorType ErrorType { get; }

    public ToolExecutionError(string message, ToolErrorType errorType = ToolErrorType.Unknown, Exception? inner = null)
        : base(message, inner)
    {
        ErrorType = errorType;
    }
}

public class SessionNotFoundError : Exception
{
    public SessionNotFoundError(string message) : base(message) { }
}

public class NotFoundError : Exception
{
    public NotFoundError(string message) : base(message) { }
}

public class InputValidationError : Exception
{
    public InputValidationError(string message) : base(message) { }
}

public class AlreadyExistsError : Exception
{
    public AlreadyExistsError(string message) : base(message) { }
}
