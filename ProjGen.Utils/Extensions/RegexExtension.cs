using System.Text.RegularExpressions;

namespace ProjGen.Utils.Extensions;

public static class RegexExtension
{
    public static Match RegexMatch(this string text,
                                   string pattern,
                                   RegexOptions options = RegexOptions.None)
        => Regex.Match(text, pattern, options);


    public static MatchCollection RegexMatches(
        this string text,
        string pattern,
        RegexOptions options = RegexOptions.None
    ) => Regex.Matches(text, pattern, options);


    public static List<string> Group(this MatchCollection matches, string group)
    {
        var list = new List<string>();
        matches.Map(m => list.Add(m.Groups[group].Value));

        return list;
    }

    public static string RegexGroup(this string text,
                                       string regex,
                                       string groupName,
                                       RegexOptions options = RegexOptions.None)
        => Regex.Match(text, regex, options).Groups[groupName].Value;


    public static List<string> RegexGroups(
        this string text,
        string regex,
        string groupName,
        RegexOptions options = RegexOptions.None
    )
    {
        var matches = Regex.Matches(text, regex, options).AsEnumerable();
        var list = new List<string>();

        foreach (var match in matches)
            list.Add(match.Groups[groupName].Value);

        return list;
    }


    public static void Map(this MatchCollection matches, Action<Match> action)
    {
        foreach (var match in matches.AsEnumerable())
            action(match);
    }


    public static bool RegexIsMatch(this string text,
                                    string regex,
                                    RegexOptions options = RegexOptions.None)
        => Regex.IsMatch(text, regex, options);


    public static string RegexReplace(this string text,
                                    string regex,
                                    string replacement,
                                    RegexOptions options = RegexOptions.None)
        => Regex.Replace(text, regex, replacement, options);

}