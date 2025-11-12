using System.Text;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;

namespace PayHelp.Application.Services;

public class FaqService : IFaqService
{
    private readonly IFaqRepository _repo;

    public FaqService(IFaqRepository repo)
    {
        _repo = repo;
    }

    public async Task<FaqEntry> RegistrarAsync(Guid ticketId, string tituloProblema, string descricaoProblema, string solucao)
    {

        if (ticketId != Guid.Empty && await _repo.ExistsForTicketAsync(ticketId))
        {

            var existente = (await _repo.SearchAsync(tituloProblema, max: 20))
                .FirstOrDefault(f => f.TicketId == ticketId);
            if (existente != null) return existente;
        }
        var entry = new FaqEntry
        {
            TicketId = ticketId,
            TituloProblema = tituloProblema,
            DescricaoProblema = descricaoProblema,
            Solucao = solucao,
            DataCriacao = DateTime.UtcNow
        };
        return await _repo.AddAsync(entry);
    }

    public async Task<IEnumerable<FaqEntry>> BuscarSimilarAsync(string texto, double limiarSimilaridade = 0.45, int max = 5)
    {
        texto = texto ?? string.Empty;


        var inputNorm = Normalizar(texto);


        var candidatos = await _repo.ListAllAsync();

        var scored = candidatos
            .Select(c => new { Entry = c, Score = ScoreFaq(inputNorm, c) })
            .Where(x => x.Score >= limiarSimilaridade)
            .OrderByDescending(x => x.Score)
            .Take(max)
            .Select(x => x.Entry)
            .ToList();
        return scored;
    }

    public Task<FaqEntry?> ObterPorTicketAsync(Guid ticketId)
        => _repo.GetByTicketAsync(ticketId);


    private static double ScoreFaq(string inputNorm, FaqEntry entry)
    {
        var desc = Normalizar(entry.DescricaoProblema ?? string.Empty);
        var tit = Normalizar(entry.TituloProblema ?? string.Empty);


        if (!string.IsNullOrWhiteSpace(desc) && desc == inputNorm) return 1.0;
        if (!string.IsNullOrWhiteSpace(tit) && tit == inputNorm) return 1.0;


        var toksIn = StemTokens(inputNorm);
        var toksDesc = StemTokens(desc);
        var toksTit = StemTokens(tit);


        var softDesc = SoftTokenSimilarity(toksIn, toksDesc);
        var softTit = SoftTokenSimilarity(toksIn, toksTit);


        var jacTokDesc = JaccardTokens(toksIn, toksDesc);
        var jacTokTit = JaccardTokens(toksIn, toksTit);


        var triDesc = JaccardTrigrams(inputNorm, desc);
        var triTit = JaccardTrigrams(inputNorm, tit);

        var soft = Math.Max(softDesc, softTit);
        var jacTok = Math.Max(jacTokDesc, jacTokTit);
        var tri = Math.Max(triDesc, triTit);


        return 0.5 * soft + 0.3 * jacTok + 0.2 * tri;
    }

    private static string Normalizar(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        s = s.Trim().ToLowerInvariant();

        var formD = s.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new StringBuilder(capacity: formD.Length);
        foreach (var ch in formD)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }
        var noDiacritics = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);

        var chars = noDiacritics.Select(c => char.IsLetterOrDigit(c) ? c : ' ').ToArray();
        var collapsed = string.Join(' ', new string(chars).Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return collapsed;
    }

    private static double SimilaridadeLevenshtein(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;
        int dist = LevenshteinDistance(a, b);
        int maxLen = Math.Max(a.Length, b.Length);
        return maxLen == 0 ? 1 : 1.0 - (double)dist / maxLen;
    }

    private static double Jaccard(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;
        var sa = a.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var sb = b.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        if (sa.Count == 0 || sb.Count == 0) return 0;
        var inter = sa.Intersect(sb).Count();
        var uni = sa.Union(sb).Count();
        return uni == 0 ? 0 : (double)inter / uni;
    }

    private static IEnumerable<string> StemTokens(string normalized)
    {
        var tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var t in tokens)
        {
            var s = StemPt(t);
            if (!string.IsNullOrWhiteSpace(s)) yield return s;
        }
    }

    private static string StemPt(string t)
    {
        if (t.Length <= 2) return t;

        if (t.EndsWith("ando") && t.Length > 4) return t[..^4] + "ar";
        if (t.EndsWith("endo") && t.Length > 4) return t[..^4] + "er";
        if (t.EndsWith("indo") && t.Length > 4) return t[..^4] + "ir";


        if (t.EndsWith('s') && t.Length > 4) return t[..^1];


        foreach (var suf in new[] { "mente", "mente", "ções", "ção" })
        {
            if (t.EndsWith(suf) && t.Length > suf.Length + 2)
            {
                return t[..^suf.Length];
            }
        }
        return t;
    }

    private static double JaccardTokens(IEnumerable<string> a, IEnumerable<string> b)
    {
        var sa = a.ToHashSet();
        var sb = b.ToHashSet();
        if (sa.Count == 0 || sb.Count == 0) return 0;
        var inter = sa.Intersect(sb).Count();
        var uni = sa.Union(sb).Count();
        return uni == 0 ? 0 : (double)inter / uni;
    }

    private static HashSet<string> Trigrams(string s)
    {
        var set = new HashSet<string>();
        if (string.IsNullOrEmpty(s)) return set;
        if (s.Length < 3) { set.Add(s); return set; }
        for (int i = 0; i <= s.Length - 3; i++)
        {
            set.Add(s.Substring(i, 3));
        }
        return set;
    }

    private static double JaccardTrigrams(string a, string b)
    {
        var sa = Trigrams(a);
        var sb = Trigrams(b);
        if (sa.Count == 0 || sb.Count == 0) return 0;
        var inter = sa.Intersect(sb).Count();
        var uni = sa.Union(sb).Count();
        return uni == 0 ? 0 : (double)inter / uni;
    }

    private static double SoftTokenSimilarity(IEnumerable<string> a, IEnumerable<string> b)
    {
        var la = a.ToList();
        var lb = b.ToList();
        if (la.Count == 0 || lb.Count == 0) return 0;

        double AvgBest(IReadOnlyList<string> src, IReadOnlyList<string> dst)
        {
            double sum = 0;
            foreach (var ta in src)
            {
                double best = 0;
                foreach (var tb in dst)
                {
                    var sim = SimilaridadeLevenshtein(ta, tb);
                    if (sim > best) best = sim;
                    if (best >= 1.0) break;
                }
                sum += best;
            }
            return sum / src.Count;
        }


        return 0.5 * (AvgBest(la, lb) + AvgBest(lb, la));
    }

    private static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length; int m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        for (int i = 0; i <= n; i++) d[i, 0] = i;
        for (int j = 0; j <= m; j++) d[0, j] = j;
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }
}
