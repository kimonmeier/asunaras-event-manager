using System.Collections.Concurrent;

namespace EventManager.Helper;

public static class FuzzyComparer
{
    public static KeyValuePair<string, double>? GetSimilarities(string source, IList<string> targets, double threshold = 0.5)
    {
        ConcurrentDictionary<string, double> scores = new ConcurrentDictionary<string, double>();

        Parallel.ForEach(targets, target =>
        {
            var score = GetUnifiedSimilarity(source, target);

            if (score >= threshold)
            {
                scores.AddOrUpdate(target, score, (_, existingScore) => Math.Max(existingScore, score));
            }
        });

        if (!scores.Any())
        {
            return null;
        }
        
        return scores.MaxBy(x => x.Value);
    }

    public static double GetUnifiedSimilarity(string source, string target)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
            return 0;

        var scores = new ConcurrentBag<double>();

        Parallel.Invoke(
            () => scores.Add(LevenshteinSimilarity(source, target)),
            () => scores.Add(JaroWinklerSimilarity(source, target)),
            () => scores.Add(TrigramOverlapSimilarity(source, target))
        );

        return scores.Any() ? scores.Average() : 0;
    }

    private static double LevenshteinSimilarity(string source, string target, CancellationToken cancellationToken = default)
    {
        int distance = LevenshteinDistance(source, target);
        int maxLength = Math.Max(source.Length, target.Length);

        return maxLength == 0 ? 1 : 1 - (double)distance / maxLength;
    }

    private static int LevenshteinDistance(string source, string target)
    {
        var dp = new int[source.Length + 1, target.Length + 1];

        Parallel.For(0, source.Length + 1, i => dp[i, 0] = i);
        Parallel.For(0, target.Length + 1, j => dp[0, j] = j);

        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                int cost = source[i - 1] == target[j - 1] ? 0 : 1;
                dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
            }
        }

        return dp[source.Length, target.Length];
    }

    private static double JaroWinklerSimilarity(string source, string target)
    {
        int m = MatchingCharacters(source, target);

        if (m == 0) return 0;

        double jaro = (1.0 / 3) * (
            (double)m / source.Length +
            (double)m / target.Length +
            (double)(m - Transpositions(source, target)) / m);

        int prefixLength = CommonPrefixLength(source, target);

        return jaro + 0.1 * prefixLength * (1 - jaro);
    }

    private static int MatchingCharacters(string source, string target)
    {
        int matchingWindow = Math.Max(source.Length, target.Length) / 2 - 1;
        var sourceMatched = new bool[source.Length];
        var targetMatched = new bool[target.Length];
        int matches = 0;

        Parallel.For(0, source.Length, i =>
        {
            for (int j = Math.Max(0, i - matchingWindow); j < Math.Min(target.Length, i + matchingWindow + 1); j++)
            {
                if (!targetMatched[j] && source[i] == target[j])
                {
                    sourceMatched[i] = true;
                    targetMatched[j] = true;
                    matches++;

                    break;
                }
            }
        });

        return matches;
    }

    private static int Transpositions(string source, string target)
    {
        int matches = MatchingCharacters(source, target);

        if (matches == 0) return 0;

        int k = 0, transpositions = 0;
        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] == target[k]) k++;
            else transpositions++;
        }

        return transpositions / 2;
    }

    private static int CommonPrefixLength(string source, string target)
    {
        int maxPrefixLength = Math.Min(4, Math.Min(source.Length, target.Length));
        int prefixLength = 0;

        for (int i = 0; i < maxPrefixLength; i++)
        {
            if (source[i] == target[i]) prefixLength++;
            else break;
        }

        return prefixLength;
    }

    private static double TrigramOverlapSimilarity(string source, string target)
    {
        var sourceTrigrams = GetTrigrams(source);
        var targetTrigrams = GetTrigrams(target);

        var intersection = sourceTrigrams.AsParallel().Intersect(targetTrigrams.AsParallel()).Count();
        var union = sourceTrigrams.AsParallel().Union(targetTrigrams.AsParallel()).Count();

        return union == 0 ? 0 : (double)intersection / union;
    }

    private static HashSet<string> GetTrigrams(string text)
    {
        var trigrams = new ConcurrentBag<string>();

        Parallel.For(0, text.Length - 2, i =>
        {
            trigrams.Add(text.Substring(i, 3));
        });

        return new HashSet<string>(trigrams);
    }
}