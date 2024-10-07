namespace Faceit_Stats_Provider.Classes
{
    public class UtilityForAnalyzer
    {
        public static string ExtractRoomIdFromUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                string[] segments = uri.Segments;
                // The room ID is always the last segment if there's no additional path, or the second-to-last if there is
                if (segments.Length >= 4 && segments[segments.Length - 2].Equals("room/", StringComparison.OrdinalIgnoreCase))
                {
                    return segments[segments.Length - 1].Trim('/');
                }
                else if (segments.Length > 4)
                {
                    return segments[segments.Length - 2].Trim('/');
                }
            }
            return null;
        }

        public static string NormalizeLabel(string label)
        {
            return label?.ToLowerInvariant().Replace("de_", "").Replace("_", "").Replace("-", "");
        }
    }
}
