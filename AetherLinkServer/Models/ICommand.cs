using System.Threading.Tasks;

namespace AetherLinkServer.Models;

public interface ICommand
{
    string Name { get; }
    Task Execute(string args, Plugin plugin);
}
