namespace EMRNext.Core.Domain.Entities.Common
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public string[] Errors { get; set; }

        public static Result<T> Ok(T data, string message = null)
        {
            return new Result<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static Result<T> Fail(string message, string[] errors = null)
        {
            return new Result<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }

    public class Result : Result<object>
    {
        public static Result Ok(string message = null)
        {
            return new Result
            {
                Success = true,
                Message = message
            };
        }

        public static new Result Fail(string message, string[] errors = null)
        {
            return new Result
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}
