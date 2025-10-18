namespace LevadS.Classes.Extensions;

internal static class StringExtensions
{
    public static string ToReadableName(this string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;

        int pos = 0;
        return ParseType(fullName, ref pos);

        static string ParseType(string s, ref int i)
        {
            int start = i;
            while (i < s.Length && s[i] != '`' && s[i] != '[' && s[i] != ',' && s[i] != ']') i++;
            var baseName = s.Substring(start, i - start).Trim();

            if (i < s.Length && s[i] == '`')
            {
                i++; // skip `
                while (i < s.Length && char.IsDigit(s[i])) i++; // skip arity digits

                // expect "[["
                if (i + 1 < s.Length && s[i] == '[' && s[i + 1] == '[')
                {
                    i += 2;
                    var args = new List<string>();
                    while (i < s.Length)
                    {
                        // skip optional leading '[' that sometimes appears around each arg
                        if (i < s.Length && s[i] == '[') i++;

                        // capture content of this argument until matching ']' (not nested)
                        int argStart = i;
                        int depth = 0;
                        while (i < s.Length)
                        {
                            if (s[i] == '[') depth++;
                            else if (s[i] == ']')
                            {
                                if (depth == 0) break;
                                depth--;
                            }
                            i++;
                        }
                        var argContent = s.Substring(argStart, i - argStart);

                        // extract type name before assembly comma (first comma at depth 0)
                        string argTypeName = ExtractTypeNameFromArgContent(argContent);

                        int innerPos = 0;
                        var parsedArg = ParseType(argTypeName, ref innerPos);
                        args.Add(parsedArg);

                        // skip closing ']' if present
                        if (i < s.Length && s[i] == ']') i++;

                        // if we've hit "]]" end of generic args list
                        if (i + 1 < s.Length && s[i] == ']' && s[i + 1] == ']') { i += 2; break; }

                        // skip comma between args
                        if (i < s.Length && s[i] == ',') { i++; continue; }

                        // otherwise break if end or unexpected
                        if (i >= s.Length || s[i] == ']') { if (i < s.Length) i++; break; }
                    }

                    return $"{baseName}<{string.Join(", ", args)}>";
                }
            }

            // not a generic or parser didn't find generic args; return baseName
            return baseName;
        }

        static string ExtractTypeNameFromArgContent(string content)
        {
            int pos = 0;
            int depth = 0;
            while (pos < content.Length)
            {
                if (content[pos] == '[') depth++;
                else if (content[pos] == ']') depth--;
                else if (content[pos] == ',' && depth == 0) break;
                pos++;
            }
            return content.Substring(0, pos).Trim();
        }
    }
}