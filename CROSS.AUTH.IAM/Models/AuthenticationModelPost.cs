namespace CROSS.AUTH.IAM.Models
{
    public class AuthenticationModelPost
    {
        public string PublicToken { get; set; } = string.Empty;
        public string AppUserId { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
    }
}
