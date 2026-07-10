using CROSS.AUTH.TOKEN;
using CROSS.AUTH.TOKEN.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Serilog;
namespace CROSS.AUTH.IAM.Middleware
{
    public class JwtAuthenticationHandler
    {
        private readonly RequestDelegate _next;
        private readonly ITokenService tokenService;

        public JwtAuthenticationHandler(
            RequestDelegate next,
            IConfiguration configuration,
            ITokenService tokenService)
        {
            _next = next;
            this.tokenService = tokenService;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
       
            var endpoint = httpContext.GetEndpoint();
            if (!httpContext.Request.Headers.TryGetValue("Jwt", out var jwtHeader))
                throw new ApplicationException("Authorization es obligatorio");

            //Insersion de los a los claims los valores del JWT
            var tokenRead = tokenService.ReadToken<ModelReadJwt>(jwtHeader);
            var claims = new List<Claim>
            {
                new("Name", tokenRead.Nombre),
                new("Matricula", tokenRead.Matricula),
            };

            claims.AddRange(
                tokenRead.Politicas.Select(p =>
                    new Claim(ClaimTypes.Role, p))
            );


            httpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity(claims, "Custom"));

            Log.Debug("Jwt Leido");

            await _next(httpContext);
     
        }
    }
}
