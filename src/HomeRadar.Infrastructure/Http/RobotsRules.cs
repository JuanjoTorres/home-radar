namespace HomeRadar.Infrastructure.Http;

/// <summary>Parsed robots.txt rules for one host, matched with longest-path precedence (Google semantics).</summary>
public sealed class RobotsRules
{
    private readonly List<(string Path, bool Allow)> _rules;

    private RobotsRules(List<(string Path, bool Allow)> rules, TimeSpan? crawlDelay)
    {
        _rules = rules;
        CrawlDelay = crawlDelay;
    }

    public TimeSpan? CrawlDelay { get; }

    public static RobotsRules AllowAll { get; } = new([], null);

    public static RobotsRules Parse(string content, string userAgentToken)
    {
        var groups = new Dictionary<string, List<(string Path, bool Allow)>>(StringComparer.OrdinalIgnoreCase);
        var delays = new Dictionary<string, TimeSpan>(StringComparer.OrdinalIgnoreCase);
        var currentAgents = new List<string>();
        var lastWasAgent = false;

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine.Split('#')[0].Trim();
            if (line.Length == 0)
            {
                continue;
            }

            var separator = line.IndexOf(':', StringComparison.Ordinal);
            if (separator < 0)
            {
                continue;
            }

            var field = line[..separator].Trim().ToLowerInvariant();
            var value = line[(separator + 1)..].Trim();
            switch (field)
            {
                case "user-agent":
                    if (!lastWasAgent)
                    {
                        currentAgents = [];
                    }

                    currentAgents.Add(value.ToLowerInvariant());
                    groups.TryAdd(value.ToLowerInvariant(), []);
                    lastWasAgent = true;
                    break;
                case "allow":
                case "disallow":
                    lastWasAgent = false;
                    if (value.Length == 0 && field == "disallow")
                    {
                        break;
                    }

                    foreach (var agent in currentAgents)
                    {
                        groups[agent].Add((value, field == "allow"));
                    }

                    break;
                case "crawl-delay":
                    lastWasAgent = false;
                    if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var seconds))
                    {
                        foreach (var agent in currentAgents)
                        {
                            delays[agent] = TimeSpan.FromSeconds(seconds);
                        }
                    }

                    break;
                default:
                    lastWasAgent = false;
                    break;
            }
        }

        var token = userAgentToken.ToLowerInvariant();
        var key = groups.Keys.FirstOrDefault(agent => agent != "*" && token.Contains(agent, StringComparison.OrdinalIgnoreCase)) ?? "*";
        var rules = groups.TryGetValue(key, out var selected) ? selected : [];
        TimeSpan? delay = delays.TryGetValue(key, out var crawlDelay) ? crawlDelay : null;
        return new RobotsRules(rules, delay);
    }

    public bool IsAllowed(string pathAndQuery)
    {
        (string Path, bool Allow)? best = null;
        foreach (var rule in _rules)
        {
            if (!Matches(rule.Path, pathAndQuery))
            {
                continue;
            }

            if (best is null || rule.Path.Length > best.Value.Path.Length || (rule.Path.Length == best.Value.Path.Length && rule.Allow))
            {
                best = rule;
            }
        }

        return best?.Allow ?? true;
    }

    private static bool Matches(string pattern, string path)
    {
        var anchored = pattern.EndsWith('$');
        var core = anchored ? pattern[..^1] : pattern;
        var parts = core.Split('*');
        var position = 0;
        for (var index = 0; index < parts.Length; index++)
        {
            var part = parts[index];
            if (index == 0)
            {
                if (!path.StartsWith(part, StringComparison.Ordinal))
                {
                    return false;
                }

                position = part.Length;
                continue;
            }

            var found = path.IndexOf(part, position, StringComparison.Ordinal);
            if (found < 0)
            {
                return false;
            }

            position = found + part.Length;
        }

        return !anchored || position == path.Length;
    }
}
