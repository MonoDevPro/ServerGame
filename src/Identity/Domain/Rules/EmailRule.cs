using System.Text.RegularExpressions;

namespace ServerGame.Domain.Rules;

internal static partial class EmailRule
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", 
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    public static partial Regex EmailRegex();
    
    public static bool IsValidEmail(string input) =>
        !string.IsNullOrWhiteSpace(input) &&
        EmailRegex().IsMatch(input);
}
