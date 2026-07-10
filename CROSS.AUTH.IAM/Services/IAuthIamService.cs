namespace CROSS.AUTH.IAM.Services
{
    public interface IAuthIamService
    {
        Task<bool> ValidateUserAsync(string idOperacion, string user, string password, string publicToken, string channel, string appUserId);
    }
}