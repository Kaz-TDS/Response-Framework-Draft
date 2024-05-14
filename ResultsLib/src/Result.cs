using System;

namespace TDS.Results
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
        public readonly T Response;
        public readonly string ErrorMessage;
        
        public Result(bool succeeded, int errorCode, T response, string errorMessage)
        {
            Succeeded = succeeded;
            ErrorCode = errorCode;
            Response = response;
            ErrorMessage = errorMessage;
        }
        public Result<T> IfSuccessful(Action<T> handler)
        {
            if(Succeeded) handler?.Invoke(Response);
            return this;
        }

        public Result<T> IfError(Action<int> handler)
        {
            if (!Succeeded) handler?.Invoke(ErrorCode);
            return this;
        }

        public static implicit operator bool(Result<T> r) => r.Succeeded;

        public static Result<T> Success(T response)
            => new Result<T>(succeeded: true, errorCode: -1,
                response: response, errorMessage: String.Empty);
        
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

        public static implicit operator bool(Result r) => r.Succeeded;

        public static Result Success()
            => new(succeeded: true, errorCode: -1);

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
        public readonly T Response;

        public Result(bool succeeded, int errorCode, T response)
        {
            Succeeded = succeeded;
            ErrorCode = errorCode;
            Response = response;
        }
        
        public Result<T> IfSuccessful(Action<T> handler)
        {
            if(Succeeded) handler?.Invoke(Response);
            return this;
        }

        public Result<T> IfError(Action<int> handler)
        {
            if (!Succeeded) handler?.Invoke(ErrorCode);
            return this;
        }

        public static implicit operator bool(Result<T> r) => r.Succeeded;

        public static Result<T> Success(T Response)
            => new Result<T>(Succeeded = true, ErrorCode = -1, Response = response);

        public override string ToString()
        {
            return Succeeded ? 
                "Result - Succeeded" :
                $"Result - Error, ErrorCode - {ErrorCode}";
        }
    }
#endif
}