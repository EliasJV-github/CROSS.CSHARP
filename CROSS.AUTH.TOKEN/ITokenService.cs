using System.Security.Claims;

namespace CROSS.AUTH.TOKEN
{
    public interface ITokenService
    {
        public string EncodeToken(IEnumerable<Claim> listClaims);
        public ILookup<string, string> ReadToken(string token);
        public T ReadToken<T>(string token) where T : new();
    }
}

