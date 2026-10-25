using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BM2.Server.Data.Configurations;

internal static class PropertyBuilderExtensions
{
    /// <summary>ISO 4217 code stored as char(3).</summary>
    public static PropertyBuilder<string> IsCurrencyCode(this PropertyBuilder<string> property) =>
        property.HasMaxLength(3).IsUnicode(false).IsFixedLength();
}
