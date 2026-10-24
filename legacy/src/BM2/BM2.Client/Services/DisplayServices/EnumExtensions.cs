using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BM2.Client.Services.DisplayServices;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        if (enumValue == null) return string.Empty;

        var memberInfo = enumValue.GetType().GetMember(enumValue.ToString());

        var enumMember = memberInfo.FirstOrDefault();
        if (enumMember == null)
        {
            return enumValue.ToString();
        }

        var displayAttribute = enumMember.GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.GetName() ?? enumValue.ToString();
    }
}