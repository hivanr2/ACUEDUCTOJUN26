using System.Security.Cryptography;
using System.Text;

namespace ACUEDUCTO.Services;

public sealed class EmailTokenService
{
    private readonly byte[] _secret;
    public EmailTokenService(string secret)
    {
        _secret = Encoding.UTF8.GetBytes(secret);
    }

    public string Issue(string userId, TimeSpan ttl)
    {
        var expires = DateTimeOffset.UtcNow.Add(ttl).ToUnixTimeSeconds();
        var payload = $"{userId}|{expires}";
        var sig = Sign(payload);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{payload}|{sig}"));
    }

    public bool TryValidate(string token, out string userId)
    {
        userId = string.Empty;
        try
        {
            var data = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = data.Split('|');
            if (parts.Length != 3) return false;
            var u = parts[0];
            var exp = long.Parse(parts[1]);
            var sig = parts[2];
            var payload = $"{u}|{exp}";
            if (!ConstantTimeEquals(Sign(payload), sig)) return false;
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp) return false;
            userId = u;
            return true;
        }
        catch { return false; }
    }

    private string Sign(string payload)
    {
        using var h = new HMACSHA256(_secret);
        var hash = h.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }

    private static bool ConstantTimeEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        int result = 0;
        for (int i = 0; i < a.Length; i++) result |= a[i] ^ b[i];
        return result == 0;
    }
}
