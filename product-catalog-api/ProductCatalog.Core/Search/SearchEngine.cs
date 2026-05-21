namespace ProductCatalog.Core.Search;

/// <summary>
/// Generic search engine using only .NET Base Class Library.
/// Supports fuzzy matching and multi-field weighted scoring.
/// </summary>
public class SearchEngine<T> where T : class
{
    private readonly List<T> _items = new();
    private readonly Dictionary<string, List<T>> _invertedIndex = new();
    private readonly Func<T, string>[] _fieldSelectors;
    private readonly double[] _fieldWeights;

    public SearchEngine(Func<T, string>[] fieldSelectors, double[] fieldWeights)
    {
        if (fieldSelectors.Length != fieldWeights.Length)
            throw new ArgumentException("Field selectors and weights must have the same length.");

        _fieldSelectors = fieldSelectors;
        _fieldWeights = fieldWeights;
    }

    public void BuildIndex(IEnumerable<T> items)
    {
        _items.Clear();
        _invertedIndex.Clear();

        foreach (var item in items)
        {
            _items.Add(item);
            IndexItem(item);
        }
    }

    private void IndexItem(T item)
    {
        for (int i = 0; i < _fieldSelectors.Length; i++)
        {
            var text = _fieldSelectors[i](item);
            if (string.IsNullOrWhiteSpace(text)) continue;

            var tokens = Tokenize(text);
            foreach (var token in tokens)
            {
                if (!_invertedIndex.ContainsKey(token))
                    _invertedIndex[token] = new List<T>();
                if (!_invertedIndex[token].Contains(item))
                    _invertedIndex[token].Add(item);
            }
        }
    }

    public void AddItem(T item)
    {
        _items.Add(item);
        IndexItem(item);
    }

    public void RemoveItem(T item)
    {
        _items.Remove(item);
        foreach (var entry in _invertedIndex.Values)
        {
            entry.Remove(item);
        }
    }

    public IEnumerable<T> Search(string query, double fuzzyThreshold = 0.8)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _items.ToList();

        var queryTokens = Tokenize(query);
        var scoredResults = new Dictionary<T, double>();

        foreach (var item in _items)
        {
            double score = ScoreItem(item, queryTokens, fuzzyThreshold);
            if (score > 0)
            {
                scoredResults[item] = score;
            }
        }

        return scoredResults
            .OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .ToList();
    }

    private double ScoreItem(T item, string[] queryTokens, double fuzzyThreshold)
    {
        double totalScore = 0;

        for (int fieldIdx = 0; fieldIdx < _fieldSelectors.Length; fieldIdx++)
        {
            var fieldText = _fieldSelectors[fieldIdx](item);
            if (string.IsNullOrWhiteSpace(fieldText)) continue;

            var fieldTokens = Tokenize(fieldText);
            double fieldScore = 0;

            foreach (var queryToken in queryTokens)
            {
                double bestTokenScore = 0;

                foreach (var fieldToken in fieldTokens)
                {
                    // Exact match
                    if (fieldToken == queryToken)
                    {
                        bestTokenScore = 1.0;
                        break;
                    }

                    // Prefix match
                    if (fieldToken.StartsWith(queryToken) || queryToken.StartsWith(fieldToken))
                    {
                        bestTokenScore = Math.Max(bestTokenScore, 0.9);
                        continue;
                    }

                    // Fuzzy match (Levenshtein)
                    var similarity = CalculateSimilarity(queryToken, fieldToken);
                    if (similarity >= fuzzyThreshold)
                    {
                        bestTokenScore = Math.Max(bestTokenScore, similarity);
                    }
                }

                fieldScore += bestTokenScore;
            }

            if (queryTokens.Length > 0)
                fieldScore /= queryTokens.Length;

            totalScore += fieldScore * _fieldWeights[fieldIdx];
        }

        return totalScore;
    }

    // --- Levenshtein Distance (pure BCL, no external libraries) ---

    private static double CalculateSimilarity(string s1, string s2)
    {
        int distance = LevenshteinDistance(s1, s2);
        int maxLen = Math.Max(s1.Length, s2.Length);
        return maxLen == 0 ? 1.0 : 1.0 - (double)distance / maxLen;
    }

    private static int LevenshteinDistance(string s1, string s2)
    {
        int n = s1.Length;
        int m = s2.Length;

        if (n == 0) return m;
        if (m == 0) return n;

        var previousRow = new int[m + 1];
        var currentRow = new int[m + 1];

        for (int j = 0; j <= m; j++)
            previousRow[j] = j;

        for (int i = 1; i <= n; i++)
        {
            currentRow[0] = i;

            for (int j = 1; j <= m; j++)
            {
                int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                currentRow[j] = Math.Min(
                    Math.Min(previousRow[j] + 1, currentRow[j - 1] + 1),
                    previousRow[j - 1] + cost
                );
            }

            // Swap rows
            var temp = previousRow;
            previousRow = currentRow;
            currentRow = temp;
        }

        return previousRow[m];
    }

    private static string[] Tokenize(string text)
    {
        return text.ToLowerInvariant()
            .Split(new[] { ' ', '-', '_', '.', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
