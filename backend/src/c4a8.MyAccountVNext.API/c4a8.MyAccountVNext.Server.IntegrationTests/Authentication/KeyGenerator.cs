using System.Security.Cryptography;

public static class KeyGenerator
{
    public static string GenerateRandom256BitKey()
    {
        byte[] key = new byte[32]; // 256 bits = 32 bytes
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }
        return Convert.ToBase64String(key);
    }
}
