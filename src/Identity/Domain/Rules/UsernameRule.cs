using System.Text.RegularExpressions;

namespace ServerGame.Domain.Rules;

internal static partial class UsernameRule
{
    // Gera em tempo de compilação um Regex estático pré-compilado
    [GeneratedRegex("^[a-zA-Z0-9._-]{3,20}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    public static partial Regex UsernameRegex();
    
    public static bool IsValidUsername(string input) =>
        !string.IsNullOrWhiteSpace(input) &&
        UsernameRegex().IsMatch(input);
}
