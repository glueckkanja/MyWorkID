using System.Text.RegularExpressions;

namespace MyWorkID.Server.Validation
{
    public static partial class AuthContextValidator
    {
        [GeneratedRegex(
           @"^c([1-9]|[1-9][0-9])$",
            RegexOptions.CultureInvariant,
            matchTimeoutMilliseconds: 1000)]
        private static partial Regex MatchIfValidAuthContext();
        public static bool IsValidAuthContext(string authContext) => MatchIfValidAuthContext().IsMatch(authContext);
    }
}
