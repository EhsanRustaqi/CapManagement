using System.Security.Claims;
using System.Text.Json;

namespace CapManagement.Client.Auth
{
    public static class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = Convert.FromBase64String(PadBase64(payload));
            var keyValuePairs =
                JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes)
                ?? new Dictionary<string, object>();

            return keyValuePairs.Select(kvp =>
                new Claim(kvp.Key, kvp.Value.ToString()!));
        }

        private static string PadBase64(string base64)
        {
            return (base64.Length % 4) switch
            {
                2 => base64 + "==",
                3 => base64 + "=",
                _ => base64
            };
        }
    }
}
