using System.Buffers.Text;
using System.Security.Cryptography;

namespace BM2.Domain.Invitations;

/// <summary>Random 256-bit link token; only its SHA-256 hash is stored.</summary>
public static class InvitationToken
{
    private const int Size = 32;

    public static (string Token, byte[] Hash) Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(Size);
        return (Base64Url.EncodeToString(bytes), SHA256.HashData(bytes));
    }

    public static bool TryHash(string? token, out byte[] hash)
    {
        hash = [];
        Span<byte> bytes = stackalloc byte[Size];
        if (string.IsNullOrEmpty(token)
            || Base64Url.GetMaxDecodedLength(token.Length) != Size
            || !Base64Url.TryDecodeFromChars(token, bytes, out var written)
            || written != Size)
        {
            return false;
        }

        hash = SHA256.HashData(bytes);
        return true;
    }
}
