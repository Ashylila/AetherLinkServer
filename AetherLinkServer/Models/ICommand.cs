using System.Threading.Tasks;

namespace AetherLinkServer.Models;

public interface ICommand
{
    string Name { get; }
    string Description { get; }
    Task Execute(string args);
}
