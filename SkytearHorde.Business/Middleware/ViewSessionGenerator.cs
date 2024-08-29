using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace SkytearHorde.Business.Middleware
{
    public class ViewSessionGenerator
    {
        private readonly IKeyValueRepository _keyValueRepository;
        private readonly IScopeProvider _scopeProvider;
        private byte[] _saltValue;

        public ViewSessionGenerator(IKeyValueRepository keyValueRepository, IScopeProvider scopeProvider)
        {
            _keyValueRepository = keyValueRepository;
            _scopeProvider = scopeProvider;
        }

        public string GetSessionId(HttpContext httpContext)
        {
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            if (string.IsNullOrWhiteSpace(userAgent)) return "UNKNOWN";

            var ip = Encoding.UTF8.GetBytes(httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty);

            var plainText = Encoding.UTF8.GetBytes(userAgent);
            var algorithm = SHA256.Create();

            byte[] plainTextWithSaltBytes = new byte[plainText.Length + _saltValue.Length + ip.Length];

            for (int i = 0; i < plainText.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainText[i];
            }
            for (int i = 0; i < ip.Length; i++)
            {
                plainTextWithSaltBytes[plainText.Length + i] = ip[i];
            }
            for (int i = 0; i < _saltValue.Length; i++)
            {
                plainTextWithSaltBytes[plainText.Length + ip.Length + i] = _saltValue[i];
            }

            return Convert.ToBase64String(algorithm.ComputeHash(plainTextWithSaltBytes));
        }

        public void GenerateSalt()
        {
            using var scope = _scopeProvider.CreateScope();
            var saltKeyValue = _keyValueRepository.Get("AnalyticsSaltValue");
            if (saltKeyValue is null)
            {
                saltKeyValue = new KeyValue
                {
                    Identifier = "AnalyticsSaltValue"
                };
            }

            if (saltKeyValue.UpdateDate.AddDays(1) > DateTime.UtcNow)
            {
                _saltValue = Convert.FromBase64String(saltKeyValue.Value!);
                return;
            }

            var rng = RandomNumberGenerator.Create();
            saltKeyValue.UpdateDate = DateTime.UtcNow;

            var salt = new byte[32];
            rng.GetBytes(salt);
            _saltValue = salt;
            saltKeyValue.Value = Convert.ToBase64String(salt);

            _keyValueRepository.Save(saltKeyValue);
            scope.Complete();
        }
    }
}
