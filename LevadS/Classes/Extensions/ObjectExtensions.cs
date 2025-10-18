using System.ComponentModel;

namespace LevadS.Classes.Extensions;

internal static class ObjectExtensions
{
    public static bool TryConvert(this object? raw, Type targetType, out object? result)
    {
        result = null;
        if (targetType == typeof(object))
        {
            result = raw;
            return true;
        }

        if (raw == null)
        {
            if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null)
            {
                result = null;
                return true;
            }
            return false;
        }

        var rawType = raw.GetType();
        if (targetType.IsAssignableFrom(rawType))
        {
            result = raw;
            return true;
        }

        var nonNullableTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;

        switch (raw)
        {
            case string s when nonNullableTarget.IsEnum:
                try
                {
                    result = Enum.Parse(nonNullableTarget, s, ignoreCase: true);
                    return true;
                }
                catch
                {
                    return false;
                }
                
            case string s when nonNullableTarget == typeof(Guid):
            {
                if (Guid.TryParse(s, out var g))
                {
                    result = g;
                    return true;
                }
                
                return false;
            }
            case string s:
            {
                var tc = TypeDescriptor.GetConverter(nonNullableTarget);
                if (tc.CanConvertFrom(typeof(string)))
                {
                    try { result = tc.ConvertFromInvariantString(s); return true; }
                    catch { return false; }
                }

                if (nonNullableTarget == typeof(string)) { result = s; return true; }

                try
                {
                    if (nonNullableTarget == typeof(int) && int.TryParse(s, out var i)) { result = i; return true; }
                    if (nonNullableTarget == typeof(long) && long.TryParse(s, out var l)) { result = l; return true; }
                    if (nonNullableTarget == typeof(double) && double.TryParse(s, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture, out var d)) { result = d; return true; }
                    if (nonNullableTarget == typeof(bool) && bool.TryParse(s, out var b)) { result = b; return true; }
                }
                catch
                {
                    // ignored
                }

                return false;
            }
            case IConvertible:
                try
                {
                    result = Convert.ChangeType(raw, nonNullableTarget, System.Globalization.CultureInfo.InvariantCulture);
                    return true;
                }
                catch
                {
                    // ignored
                }

                break;
        }

        var conv = TypeDescriptor.GetConverter(rawType);
        if (conv.CanConvertTo(nonNullableTarget))
        {
            try
            {
                result = conv.ConvertTo(raw, nonNullableTarget);
                return true;
            }
            catch
            {
                // ignored
            }
        }

        var conv2 = TypeDescriptor.GetConverter(nonNullableTarget);
        if (conv2.CanConvertFrom(rawType))
        {
            try
            {
                result = conv2.ConvertFrom(raw);
                return true;
            }
            catch
            {
                // ignored
            }
        }

        return false;
    }
}