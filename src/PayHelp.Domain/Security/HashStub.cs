using System.Security.Cryptography;
using System.Text;

namespace PayHelp.Domain.Security;




public static class HashStub
{
    public static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    public static bool Verify(string input, string hash) => string.Equals(Hash(input), hash, StringComparison.OrdinalIgnoreCase);
}
