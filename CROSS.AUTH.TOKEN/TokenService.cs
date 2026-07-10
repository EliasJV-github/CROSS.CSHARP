using CROSS.SECRYPT;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Serilog;

namespace CROSS.AUTH.TOKEN
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ClaimNameAttribute : Attribute
    {
        public string Name { get; }
        public ClaimNameAttribute(string name) => Name = name;
    }

    public class TokenService : ITokenService
    {
        private readonly JwtSettings jwtSettings;
        private readonly IManagerSecrypt secrypt;

        public TokenService(IConfiguration configuration, IManagerSecrypt secrypt)
        {
            var jwt = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwt);
            this.jwtSettings = jwt;
            this.secrypt = secrypt;
            jwtSettings.Key = secrypt.Desencriptar(jwtSettings.Key);
        }

        public string EncodeToken(IEnumerable<Claim> listClaims)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claimList = listClaims.ToList();
                if (!claimList.Any(c => c.Type == JwtRegisteredClaimNames.Jti))
                    claimList.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

                if (!claimList.Any(c => c.Type == JwtRegisteredClaimNames.Iat))
                    claimList.Add(new Claim(
                        JwtRegisteredClaimNames.Iat,
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                        ClaimValueTypes.Integer64));

                var token = new JwtSecurityToken(
                    issuer: jwtSettings.Issuer,
                    audience: jwtSettings.Audience,
                    claims: claimList,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(jwtSettings.Duration),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Log.Error($"Error al generar el token JWT - {ex.Message}", ex);
                throw new ApplicationException($"Error al generar el token JWT", ex);
            }
        }


        public ILookup<string, string> ReadToken(string token)
        {
            try
            {
                var principal = ValidateAndGetPrincipal(token, jwtSettings.Key, jwtSettings.Issuer, jwtSettings.Audience);
                return principal.Claims.ToLookup(c => c.Type, c => c.Value);
            }
            catch (Exception ex)
            {
                Log.Error($"Error al leer el Token - {ex.Message}", ex);
                throw new ApplicationException($"Error al leer el Token", ex);
            }
        }



        public T ReadToken<T>(string token) where T : new()
        {
            try
            {
                var principal = ValidateAndGetPrincipal(token, jwtSettings.Key, jwtSettings.Issuer, jwtSettings.Audience);
                var result = new T();
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var prop in properties)
                {
                    var claimName = prop.GetCustomAttribute<ClaimNameAttribute>()?.Name
                                    ?? prop.Name;

                    var values = principal.Claims
                        .Where(c => c.Type.Equals(claimName, StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Value)
                        .ToList();

                    if (values.Count == 0) continue;
                    if (prop.PropertyType == typeof(List<string>))
                    {
                        prop.SetValue(result, values);
                    }
                    // string → primer valor
                    else if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(result, values.First());
                    }
                    else
                    {
                        var converted = Convert.ChangeType(values.First(), prop.PropertyType);
                        prop.SetValue(result, converted);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Error al leer el Token - {ex.Message}", ex);
                throw new ApplicationException($"Error al leer el Token", ex);
            }
        }



        private static ClaimsPrincipal ValidateAndGetPrincipal(string token, string secretKey, string? issuer, string? audience)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = issuer is not null,
                ValidIssuer = issuer,
                ValidateAudience = audience is not null,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromHours(1)
            };

            return new JwtSecurityTokenHandler().ValidateToken(token, validationParams, out _);
        }



    }
}
