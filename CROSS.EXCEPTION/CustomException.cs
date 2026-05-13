namespace CROSS.EXCEPTION_HANDLER
{
    public class CustomException : Exception
    {
        public string StatusCode { get; }
        public string MessageLog { get; }
        public CustomException(string message, string MessageLog = "Error inesperado", string statusCode = "99")
            : base(message)
        {
            this.MessageLog = MessageLog;
            this.StatusCode = statusCode;
        }
    }
}

