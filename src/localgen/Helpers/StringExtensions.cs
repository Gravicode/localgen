namespace localgen.Helpers
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            var words = str.Split(new[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
            words = words.Select(word => char.ToUpper(word[0]) + word.Substring(1)).ToArray();
            return string.Join(string.Empty, words);
        }
        public static string RemovePrefix(this string s, int prefixLen)
        {
            if (s.Length < prefixLen)
            {
                return string.Empty;
            }
            return s.Substring(prefixLen);
        }
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
        public static bool Contains(this string source, string[] toChecks, StringComparison comp)
        {
            foreach (var toCheck in toChecks)
            {
                bool res = (source?.IndexOf(toCheck, comp) >= 0);
                if (res) return res;
            }
            return false;
        }
       
    }
}
