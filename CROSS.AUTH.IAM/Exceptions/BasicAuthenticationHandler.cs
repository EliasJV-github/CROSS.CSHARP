using CROSS.APIREST;
using CROSS.AUTH.IAM.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Serilog;

namespace CROSS.AUTH.IAM.Middleware
{
    public class BasicAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IApiRest apiRest;
        private readonly IamSetting iamSetting;

        public BasicAuthenticationMiddleware(
            RequestDelegate next,
            IApiRest apiRest,
            IConfiguration configuration)
        {
            _next = next;
            this.apiRest = apiRest;

            iamSetting = new IamSetting();
            configuration.GetSection("IamApi").Bind(iamSetting);
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {

            var endpoint = httpContext.GetEndpoint();
            if (!httpContext.Request.Headers.TryGetValue("Authorization", out var authorization))
                throw new ApplicationException("Authorization es obligatorio");

            if (!httpContext.Request.Headers.TryGetValue("publicToken", out var publicToken))
                throw new ApplicationException("publicToken es obligatorio");

            if (!httpContext.Request.Headers.TryGetValue("channel", out var channel))
                throw new ApplicationException("channel es obligatorio");

            if (!httpContext.Request.Headers.TryGetValue("AppUserId", out var appUserId))
                throw new ApplicationException("UserId es obligatorio");

            var authHeader = AuthenticationHeaderValue.Parse(authorization);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? "");
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');

            string userIam = credentials[0];
            string passIam = credentials[1];

            AuthenticationModelPost request = new AuthenticationModelPost()
            {
                PublicToken= publicToken,
                AppUserId= appUserId,
                Date = DateTime.Now.ToString("yyyyMMdd"),
                Channel = channel
            };

            AuthenticationModel response =
                await apiRest.PostAsync<AuthenticationModel>(
                    httpContext.TraceIdentifier,
                    iamSetting.UrlBase + iamSetting.Method,
                    request,
                    true,
                    userIam,
                    passIam);

            if (response.State != "00")
                throw new ApplicationException("Error en la autenticación");

            var claims = new List<Claim>
            {
                new("User", response.Data.UserName)
            };

            httpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity(claims, "Custom"));

            Log.Debug("Login completado");

            await _next(httpContext);
    
        }
    }
}
