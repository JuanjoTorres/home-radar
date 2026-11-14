using System.Security.Cryptography;
using System.Text;

namespace HomeRadar.Domain.ValueObjects;

public readonly record struct ContentHash(string Value)
{
    public static ContentHash Of(string canonicalContent)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalContent));
        return new ContentHash(Convert.ToHexStringLower(bytes));
    }

    public override string ToString() => Value;
}
