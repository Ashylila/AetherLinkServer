
namespace AetherLinkServer.Models;
public interface ICommand
{
    string Name { get; }
    void Execute(string args);
}