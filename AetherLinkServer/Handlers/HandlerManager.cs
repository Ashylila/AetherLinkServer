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

    public bool TryInterruptRunningHandlers()
    {
        if(_handlers.Any(h => (h.IsEnabled && !h.CanBeInterrupted)))
            return false;
        foreach (var handler in _handlers)
        {

            if (handler.IsEnabled && handler.CanBeInterrupted)
            {
                handler.Disable();
            }
        }
        return true;
    }

    public IEnumerable<HandlerBase> GetRunningHandlers()
        => _handlers.Where(h => h.IsEnabled);
}
