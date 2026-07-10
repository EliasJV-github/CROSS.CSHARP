namespace CROSS.AUTH.IAM.Models
{
    public class AuthenticationDataModel
    {
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string PublicToken { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Date { get; set; } = string.Empty;
    }
}
