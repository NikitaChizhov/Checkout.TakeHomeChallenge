using System.Diagnostics.CodeAnalysis;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;

public class Result
{
    public Result(bool success, string? reason, FailureCode? code)
    {
        Success = success;
        Reason = reason;
        Code = code;
    }

    /// <summary>
    /// True if operation was successful and Value is set.
    /// False otherwise (in which case Reason should describe what happened)
    /// </summary>
    public bool Success { get; }
    
    /// <summary>
    /// Description of what went wrong. Null if operation was successful 
    /// </summary>
    [MemberNotNullWhen(false, nameof(Success))]
    public string? Reason { get; }
    
    /// <summary>
    /// Codes for expected possible failures.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Success))]
    public FailureCode? Code { get; }
    
    public static Result Fail(string reason, FailureCode code = FailureCode.Default) 
        => new(false, reason, code);

    public static Result Successful() => new Result(true, null, null);
}

public sealed class Result<T> : Result
{
    public Result(T value) : base(true, null, null)
    {
        Value = value;
    }

    public Result(T? value, bool success, string? reason, FailureCode code) 
        : base(success, reason, code)
    {
        Value = value;
    }

    public new static Result<T> Fail(string reason, FailureCode code = FailureCode.Default) 
        => new(default, false, reason, code);

    /// <summary>
    /// Actual result of the operation
    /// </summary>
    public T? Value { get; }

    public static implicit operator Result<T>(T value) => new(value);
}