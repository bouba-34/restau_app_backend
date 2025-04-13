namespace backend.Api.Models.Responses
{
    public class ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public ErrorResponse(string message)
        {
            Message = message;
        }

        public ErrorResponse(string message, List<string> errors)
        {
            Message = message;
            Errors = errors;
        }

        public ErrorResponse(string message, string error)
        {
            Message = message;
            Errors.Add(error);
        }
    }
}