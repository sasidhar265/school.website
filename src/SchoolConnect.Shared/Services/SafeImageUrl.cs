namespace SchoolConnect.Shared.Services;

public static class SafeImageUrl
{
    public static bool IsAllowed(string value)
    {
        var candidate = value.Trim();
        if (candidate.Length == 0 || candidate.StartsWith("//", StringComparison.Ordinal)
            || candidate.Contains('\\') || candidate.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        if (candidate.StartsWith("/", StringComparison.Ordinal))
        {
            return true;
        }

        if (Uri.TryCreate(candidate, UriKind.Absolute, out var absolute))
        {
            return string.Equals(absolute.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        }

        return Uri.TryCreate(candidate, UriKind.Relative, out _)
            && !candidate.Contains(":", StringComparison.Ordinal);
    }
}
