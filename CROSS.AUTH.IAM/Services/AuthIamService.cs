using CROSS.APIREST;
using CROSS.AUTH.IAM.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
namespace CROSS.AUTH.IAM.Services
{
    public class AuthIamService : IAuthIamService
    {
        private readonly IApiRest apiRest;
        private readonly IamSetting iamSetting;

        public AuthIamService(IApiRest apiRest, IConfiguration configuration)
        {
            this.apiRest = apiRest;
            IamSetting iamSettingLocal = new ();
            configuration.GetSection("IamApi").Bind(iamSettingLocal);
            this.iamSetting = iamSettingLocal;
        }

        public async Task<bool> ValidateUserAsync(string idOperacion, string user, string password, string publicToken, string channel, string appUserId)
        {
            try
            {
                var request = new
                {
                    publicToken,
                    appUserId,
                    date = DateTime.Now.ToString("yyyyMMdd"),
                    channel,
                };
                AuthenticationModel response = await apiRest.PostAsync<AuthenticationModel>(idOperacion, iamSetting.UrlBase + iamSetting.Method, request, true, user, password);
                bool result = response.State == "00";
                return result;
            }
            catch (Exception ex)
            {
                Log.Error("Error al validar el usuario IAM {ex}",ex.Message,ex);
                return false;
            }
        }
    }
}
