namespace FCR.Bll.Common
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        // Success response
        public static ServiceResponse<T> SuccessResponse(T data, string message = "Operation successful")
        {
            return new ServiceResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        // Error response
        public static ServiceResponse<T> ErrorResponse(string message, List<string>? errors = null)
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        // Single error
        public static ServiceResponse<T> ErrorResponse(string message, string error)
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }
    }
}