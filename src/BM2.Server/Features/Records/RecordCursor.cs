using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace BM2.Server.Features.Records;

/// <summary>Opaque keyset cursor: the sort key of the last record on a page.</summary>
internal readonly record struct RecordCursor(DateOnly OccurredOn, DateTimeOffset CreatedAt, Guid Id)
{
    public string Encode() => Base64Url.EncodeToString(Encoding.UTF8.GetBytes(
        string.Create(CultureInfo.InvariantCulture, $"{OccurredOn.DayNumber}|{CreatedAt.UtcTicks}|{Id:N}")));

    public static bool TryDecode(string value, out RecordCursor cursor)
    {
        cursor = default;
        byte[] bytes;
        try
        {
            bytes = Base64Url.DecodeFromChars(value);
        }
        catch (FormatException)
        {
            return false;
        }

        var parts = Encoding.UTF8.GetString(bytes).Split('|');
        if (parts.Length != 3
            || !int.TryParse(parts[0], CultureInfo.InvariantCulture, out var day)
            || day < DateOnly.MinValue.DayNumber || day > DateOnly.MaxValue.DayNumber
            || !long.TryParse(parts[1], CultureInfo.InvariantCulture, out var ticks)
            || ticks < DateTimeOffset.MinValue.UtcTicks || ticks > DateTimeOffset.MaxValue.UtcTicks
            || !Guid.TryParseExact(parts[2], "N", out var id))
        {
            return false;
        }

        cursor = new RecordCursor(DateOnly.FromDayNumber(day), new DateTimeOffset(ticks, TimeSpan.Zero), id);
        return true;
    }
}
