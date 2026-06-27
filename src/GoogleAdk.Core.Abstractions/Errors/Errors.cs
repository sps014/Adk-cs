namespace GoogleAdk.Core.Abstractions.Errors;

/// <summary>
/// Categorizes why a tool failed, so callers can react differently to
/// validation problems, execution failures, and timeouts.
/// </summary>
public enum ToolErrorType
{
    /// <summary>The cause of the failure is unknown or unclassified.</summary>
    Unknown,

    /// <summary>The tool arguments failed validation.</summary>
    Validation,

    /// <summary>The tool ran but failed during execution.</summary>
    Execution,

    /// <summary>The tool did not complete within its allotted time.</summary>
    Timeout
}

/// <summary>
/// Raised when a tool fails to execute, carrying a <see cref="ToolErrorType"/>
/// classification alongside the standard exception details.
/// </summary>
public sealed class ToolExecutionError : Exception
{
    /// <summary>The category of the failure.</summary>
    public ToolErrorType ErrorType { get; }

    /// <summary>Creates a new <see cref="ToolExecutionError"/>.</summary>
    /// <param name="message">A description of the failure.</param>
    /// <param name="errorType">The failure category.</param>
    /// <param name="inner">The underlying exception, if any.</param>
    public ToolExecutionError(string message, ToolErrorType errorType = ToolErrorType.Unknown, Exception? inner = null)
        : base(message, inner)
    {
        ErrorType = errorType;
    }
}

/// <summary>Raised when a requested session cannot be found.</summary>
public class SessionNotFoundError : Exception
{
    /// <summary>Creates a new <see cref="SessionNotFoundError"/>.</summary>
    /// <param name="message">A description of the failure.</param>
    public SessionNotFoundError(string message) : base(message) { }
}

/// <summary>Raised when a requested resource cannot be found.</summary>
public class NotFoundError : Exception
{
    /// <summary>Creates a new <see cref="NotFoundError"/>.</summary>
    /// <param name="message">A description of the failure.</param>
    public NotFoundError(string message) : base(message) { }
}

/// <summary>Raised when input fails validation.</summary>
public class InputValidationError : Exception
{
    /// <summary>Creates a new <see cref="InputValidationError"/>.</summary>
    /// <param name="message">A description of the validation failure.</param>
    public InputValidationError(string message) : base(message) { }
}

/// <summary>Raised when attempting to create a resource that already exists.</summary>
public class AlreadyExistsError : Exception
{
    /// <summary>Creates a new <see cref="AlreadyExistsError"/>.</summary>
    /// <param name="message">A description of the conflict.</param>
    public AlreadyExistsError(string message) : base(message) { }
}
