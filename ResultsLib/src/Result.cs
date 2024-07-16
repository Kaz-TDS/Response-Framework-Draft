using System;

namespace Tripledot.Results
{
#if DEBUG
    public readonly struct Result
    {
        public readonly bool Succeeded;
        public readonly int ErrorCode;
        public readonly string ErrorMessage;

        public Result(bool succeeded, int errorCode, string errorMessage)
        {
            Succeeded = succeeded;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
        
        #if RESULT_CALLBACKS
        public Result IfSuccessful(Action handler)
        {
            if(Succeeded) handler?.Invoke();
            return this;
        }

        public Result IfError(Action<int> handler)
        {
            if (!Succeeded) handler?.Invoke(ErrorCode);
            return this;
        }
        #endif

        public static implicit operator bool(Result r) => r.Succeeded;

        public static Result Success()
            => new Result(succeeded: true, errorCode: -1, errorMessage: String.Empty);

        public override string ToString()
        {
            return Succeeded ? 
                "Result - Succeeded" :
                $"Result - Error, ErrorCode - {ErrorCode}, ErrorMessage - {ErrorMessage}";
        }
    }

    public readonly struct Result<T>
    {
        public readonly bool Succeeded;
        public readonly int ErrorCode;
        public readonly T Value;
        public readonly string ErrorMessage;
        
        public Result(bool succeeded, int errorCode, T value, string errorMessage)
        {
            Succeeded = succeeded;
            ErrorCode = errorCode;
            Value = value;
            ErrorMessage = errorMessage;
        }

        #if RESULT_CALLBACKS
        
        public Result<T> IfSuccessful(Action<T> handler)
        {
            if(Succeeded) handler?.Invoke(Value);
            return this;
        }

        public Result<T> IfError(Action<int> handler)
        {
            if (!Succeeded) handler?.Invoke(ErrorCode);
            return this;
        }

        #endif

        public static implicit operator bool(Result<T> r) => r.Succeeded;

        public static Result<T> Success(T value)
            => new Result<T>(succeeded: true, errorCode: -1,
                value: value, errorMessage: String.Empty);
        
        public override string ToString()
        {
            return Succeeded ? 
                "Result - Succeeded" :
                $"Result - Error, ErrorCode - {ErrorCode}, ErrorMessage - {ErrorMessage}";
        }
    }

#else

    public readonly struct Result
    {
        public readonly bool Succeeded;
        public readonly int ErrorCode;

        public Result(bool succeeded, int errorCode)
        {
            Succeeded = succeeded;
            ErrorCode = errorCode;
        }

        #if RESULT_CALLBACKS
        public Result IfSuccessful(Action handler)
        {
            if(Succeeded) handler?.Invoke();
            return this;
        }

        public Result IfError(Action<int> handler)
        {
            if (!Succeeded) handler?.Invoke(ErrorCode);
            return this;
        }
        #endif

        public static implicit operator bool(Result r) => r.Succeeded;

        public static Result Success()
            => new Result(succeeded: true, errorCode: -1);

        public override string ToString()
        {
            return Succeeded ? 
                "Result - Succeeded" :
                $"Result - Error, ErrorCode - {ErrorCode}";
        }
    }

    public readonly struct Result<T>
    {
        public readonly bool Succeeded;
        public readonly int ErrorCode;
        public readonly T Value;

        public Result(bool succeeded, int errorCode, T value)
        {
            Succeeded = succeeded;
            ErrorCode = errorCode;
            Value = value;
        }
        
        public Result<T> IfSuccessful(Action<T> handler)
        {
            if(Succeeded) handler?.Invoke(Value);
            return this;
        }

        public Result<T> IfError(Action<int> handler)
        {
            if (!Succeeded) handler?.Invoke(ErrorCode);
            return this;
        }

        public static implicit operator bool(Result<T> r) => r.Succeeded;
        
        public static Result<T> Success(T response)
            => new Result<T>(succeeded: true, errorCode: -1, value: response);

        public override string ToString()
        {
            return Succeeded ? 
                "Result - Succeeded" :
                $"Result - Error, ErrorCode - {ErrorCode}";
        }
    }
#endif
}