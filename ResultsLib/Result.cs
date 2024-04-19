using System.Runtime.CompilerServices;

namespace TDS.Results;

#if DEBUG

public readonly struct Result
{
    public readonly bool Succeeded { get; init; }
    public readonly int ErrorCode { get; init; }
    public readonly string ErrorMessage { get; init; }

    public static implicit operator bool(Result r) => r.Succeeded;

    public static Result Success()
        => new() {Succeeded = true, ErrorCode = -1, ErrorMessage = String.Empty };
    
    public override string ToString()
    {
        return $"Result - {Succeeded}, ErrorCode - {ErrorCode}";
    }
}

public readonly struct Result<T>
{
    public readonly bool Succeeded { get; init; }
    public readonly int ErrorCode { get; init; }
    public readonly T Response { get; init; }
    public readonly string ErrorMessage { get; init; }
    
    public static implicit operator bool(Result<T> r) => r.Succeeded;
    
    public static Result<T> Success(T response)
        => new() {Succeeded = true, ErrorCode = -1, Response = response, ErrorMessage = String.Empty };
}

#else 

public readonly partial struct Result
{
    public readonly bool Succeeded { get; init; }
    public readonly int ErrorCode { get; init; }

    public static implicit operator bool(Result r) => r.Succeeded;

    public static Result Success()
        => new() {Succeeded = true, ErrorCode = -1};
}

public readonly partial struct Result<T>
{
    public readonly bool Succeeded { get; init; }
    public readonly int ErrorCode { get; init; }
    public readonly T Response { get; init; }
    
    public static implicit operator bool(Result<T> r) => r.Succeeded;

    public static Result Success(T Response)
        => new() {Succeeded = true, ErrorCode = -1, Response = response };
}

#endif