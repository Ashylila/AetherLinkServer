using System.Collections.Generic;
using System.Linq;

namespace AetherLinkServer.Handlers;

public sealed class HandlerManager
{
    private readonly List<HandlerBase> _handlers = [];

    public void Register(HandlerBase handler)
    {
        if (!_handlers.Contains(handler))
            _handlers.Add(handler);
    }

    public void Unregister(HandlerBase handler)
    {
        _handlers.Remove(handler);
    }

    public (bool result, string reason) TryInterruptRunningHandlers()
    {
        if(_handlers.Any(h => (h.IsEnabled && !h.CanBeInterrupted)))
            return (false, $"{_handlers.First(h => h.IsEnabled && !h.CanBeInterrupted).HandlerName} cant be interrupted.");
        foreach (var handler in _handlers)
        {

            if (handler.IsEnabled && handler.CanBeInterrupted)
            {
                handler.Disable();
            }
        }
        return (true, "Handlers successfully interrupted");
    }

    public IEnumerable<HandlerBase> GetRunningHandlers()
        => _handlers.Where(h => h.IsEnabled);
}
