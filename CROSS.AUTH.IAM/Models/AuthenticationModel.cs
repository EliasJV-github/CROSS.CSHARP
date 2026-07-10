namespace CROSS.AUTH.IAM.Models
{
    public class AuthenticationModel
    {
        public string State { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public AuthenticationDataModel? Data { get; set; }
    }
}
