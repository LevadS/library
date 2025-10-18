namespace LevadS.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Validates a topic pattern.
    /// Parts are separated by <paramref name="separator"/> (default ':').
    /// Valid parts:
    ///  - literal (any text not containing wildcards or braces)
    ///  - "#" repeated one or more times (each '#' matches exactly one part)
    ///  - "*" (matches zero or more parts) - must be single char part
    ///  - "+" (matches one or more parts) - must be single char part
    ///  - capture "{name}" or "{name:type}" where name is an identifier and type is one of supported types
    /// Rules:
    ///  - "*" and "+" must appear alone as a part (no repetition like "**" or "++")
    ///  - "#" may repeat (e.g. "##" means exactly two parts)
    ///  - Parts cannot be empty
    ///  - Capture names must be unique
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="error"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static bool IsValidTopicPattern(this string pattern, out string? error, char separator = ':')
    {
        error = null;
        ArgumentNullException.ThrowIfNull(pattern);
        if (pattern.Length == 0) { error = "Pattern is empty."; return false; }

        var supportedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "int", "int32", "long", "int64", "double", "bool", "boolean", "string", "guid" };

        // Use splitter that ignores separators inside {...} so captures with ":" are not broken.
        var parts = SplitPatternParts(pattern, separator);
        var seenCaptureNames = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < parts.Length; i++)
        {
            var p = parts[i];
            if (string.IsNullOrEmpty(p))
            {
                error = $"Empty part at index {i}.";
                return false;
            }

            switch (p)
            {
                // exact "*" (single) is allowed
                case "*":
                // exact "+" (single) is allowed
                case "+":
                    continue;
            }

            // one or more '#' characters is allowed (each '#' = one part)
            if (p.All(c => c == '#')) continue;

            // If part contains any brace, require it to be exactly one capture ("{name}" or "{name:type}")
            if (p.Contains('{') || p.Contains('}'))
            {
                if (IsCapturePart(p, out var name, out var typeName))
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        error = $"Capture at index {i} has empty name.";
                        return false;
                    }

                    if (!IsValidIdentifier(name))
                    {
                        error = $"Invalid capture name '{name}' at index {i}.";
                        return false;
                    }

                    if (!string.IsNullOrEmpty(typeName) && !supportedTypes.Contains(typeName.Trim()))
                    {
                        error = $"Unsupported capture type '{typeName}' for '{name}' at index {i}.";
                        return false;
                    }

                    if (!seenCaptureNames.Add(name))
                    {
                        error = $"Duplicate capture name '{name}' at index {i}.";
                        return false;
                    }
                    
                    continue;
                }
                
                error = $"Invalid capture syntax in part '{p}' at index {i}.";
                return false;
            }

            // invalid if part contains any wildcard mixed into a literal
            if (!p.Any(c => c is '*' or '+' or '#')) continue;
            {
                var bad = p.First(c => c is '*' or '+' or '#');
                error = $"Invalid use of '{bad}' in literal part '{p}' at index {i}.";
                return false;
            }

            // otherwise it's a literal -> allowed
        }

        return true;
    }
    
    /// <summary>
    /// Matches a topic against a pattern and fills captured values into the provided dictionary.
    /// Pattern parts are separated by <paramref name="separator"/> (default ':').
    /// Capture template: {name:type} (example: {id:int}).
    /// Supported types: int, long, double, bool, string, guid
    /// Captures are stored in the provided IDictionary&lt;string, object&gt; keyed by capture name.
    /// </summary>
    public static bool MatchesTopicPattern(this string topic, string pattern, IDictionary<string, object> capturedValues, char separator = ':')
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(capturedValues);

        // ensure caller dictionary is cleared before matching
        capturedValues.Clear();

        var tParts = topic.Split(separator);
        // Use a custom splitter that ignores separator characters inside capture braces { ... }
        var pParts = SplitPatternParts(pattern, separator);

        var tokens = pParts.Select(p => p switch
            {
                "*" => new Token(TokenKind.Star),
                "+" => new Token(TokenKind.Plus),
                _ => p.Length > 0 && p.All(c => c == '#')
                    ? new Token(TokenKind.Hash, count: p.Length)
                    :
                    IsCapturePart(p, out var name, out var typeName)
                        ?
                        new Token(TokenKind.Capture, text: name, typeName: typeName)
                        : new Token(TokenKind.Literal, text: p)
            }
        ).ToArray();

        var captures = new Dictionary<string, object>(StringComparer.Ordinal);
        // stack to restore previous capture state on backtrack
        var stateStack = new Stack<(string Key, bool Existed, object? OldValue)>();

        return Match(0, 0);

        bool Match(int ti, int pi)
        {
            while (true)
            {
                if (ti == tParts.Length && pi == tokens.Length)
                {
                    // success - copy captures into caller dictionary
                    capturedValues.Clear();
                    foreach (var kv in captures) capturedValues[kv.Key] = kv.Value;
                    return true;
                }

                if (pi == tokens.Length) return false;

                var tok = tokens[pi];
                switch (tok.Kind)
                {
                    case TokenKind.Literal:
                        if (ti >= tParts.Length || tParts[ti] != tok.Text) return false;
                        ti += 1;
                        pi += 1;
                        continue;

                    case TokenKind.Hash:
                        if (ti + tok.Count > tParts.Length) return false;
                        ti += tok.Count;
                        pi += 1;
                        continue;

                    case TokenKind.Star:
                        for (var take = 0; take <= tParts.Length - ti; ++take)
                        {
                            if (Match(ti + take, pi + 1)) return true;
                        }

                        return false;

                    case TokenKind.Plus:
                        for (var take = 1; take <= tParts.Length - ti; ++take)
                        {
                            if (Match(ti + take, pi + 1)) return true;
                        }

                        return false;

                    case TokenKind.Capture:
                        if (ti >= tParts.Length) return false;
                        if (!TryConvert(tParts[ti], tok.TypeName, out var val)) return false;
                        var key = tok.Text ?? string.Empty;
                        var existed = captures.TryGetValue(key, out var old);
                        // set/overwrite current capture
                        captures[key] = val!;
                        stateStack.Push((key, existed, old));

                        if (Match(ti + 1, pi + 1)) return true;

                        // restore on backtrack
                        var st = stateStack.Pop();
                        if (st.Existed)
                            captures[st.Key] = st.OldValue!;
                        else
                            captures.Remove(st.Key);

                        return false;

                    default:
                        return false;
                }
            }
        }
    }

    // Helper: split pattern by separator but ignore separators inside {...} captures.
    private static string[] SplitPatternParts(this string pattern, char separator)
    {
        var parts = new List<string>();
        var depth = 0;
        var last = 0;
        for (var i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];
            switch (c)
            {
                case '{':
                    depth++;
                    break;
                case '}':
                {
                    if (depth > 0) depth--;
                    break;
                }
                default:
                {
                    if (c == separator && depth == 0)
                    {
                        parts.Add(pattern.Substring(last, i - last));
                        last = i + 1;
                    }

                    break;
                }
            }
        }
        parts.Add(pattern[last..]);
        return parts.ToArray();
    }

    private enum TokenKind { Literal, Hash, Star, Plus, Capture }

    private readonly struct Token(TokenKind kind, string? text = null, int count = 0, string? typeName = null)
    {
        public TokenKind Kind { get; } = kind;
        public string? Text { get; } = text;
        public int Count { get; } = count;
        public string? TypeName { get; } = typeName;
    }

    private static bool IsCapturePart(this string part, out string name, out string typeName, char separator = ':')
    {
        name = string.Empty;
        typeName = "string";

        if (part is not ['{', _, _, ..] || part[^1] != '}') return false;
        var inner = part.Substring(1, part.Length - 2);
        var idx = inner.IndexOf(separator);
        if (idx >= 0)
        {
            name = inner[..idx];
            typeName = inner[(idx + 1)..];
            return !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(typeName);
        }
        
        name = inner;
        typeName = "string";
        return !string.IsNullOrEmpty(name);
    }

    private static bool TryConvert(string text, string? typeName, out object? value)
    {
        value = null;
        typeName ??= "string";
        switch (typeName.Trim().ToLowerInvariant())
        {
            case "int":
            case "int32":
                if (!int.TryParse(text, out var i)) return false;
                value = i; return true;
            case "long":
            case "int64":
                if (!long.TryParse(text, out var l)) return false;
                value = l; return true;
            case "double":
                if (!double.TryParse(text,
                        System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands,
                        System.Globalization.CultureInfo.InvariantCulture, out var d)) return false;
                value = d; return true;
            case "bool":
            case "boolean":
                if (!bool.TryParse(text, out var b)) return false;
                value = b; return true;
            case "guid":
                if (!Guid.TryParse(text, out var g)) return false;
                value = g; return true;
            case "string":
                value = text;
                return true;
            default:
                // unknown type - fallback to string
                value = text;
                return true;
        }
    }

    private static bool IsValidIdentifier(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        var first = s[0];
        if (!(char.IsLetter(first) || first == '_')) return false;
        for (var i = 1; i < s.Length; i++)
        {
            var ch = s[i];
            if (!(char.IsLetterOrDigit(ch) || ch == '_')) return false;
        }
        return true;
    }
}