using System.Collections.Concurrent;

namespace PayHelp.WebApp.Mvc.Services;

public interface ITriageTracker
{
    void MarkStarted(Guid ticketId);
    void Clear(Guid ticketId);
    bool IsTriaging(Guid ticketId);
    IReadOnlyCollection<Guid> GetAllTriaging();
}

public class TriageTracker : ITriageTracker
{
    private readonly ConcurrentDictionary<Guid, byte> _triaging = new();

    public void MarkStarted(Guid ticketId) => _triaging[ticketId] = 1;
    public void Clear(Guid ticketId) => _triaging.TryRemove(ticketId, out _);
    public bool IsTriaging(Guid ticketId) => _triaging.ContainsKey(ticketId);
    public IReadOnlyCollection<Guid> GetAllTriaging() => _triaging.Keys.ToList();
}
