using System.Collections.Generic;
using System.Reflection;
using AetherLinkServer.Models;
using System.Linq;
using System;
using AetherLinkServer.DalamudServices;
using Dalamud.Plugin.Services;
using System.Threading.Tasks;

namespace AetherLinkServer.Handlers;

public class CommandHandler
{
    private IPluginLog Logger => Svc.Log;
    private readonly Dictionary<string, ICommand> _commands = new();

    public CommandHandler(Plugin plugin)
    {
        LoadCommands();
        Plugin.server.OnCommandReceived += HandleCommand;
    }

    private void LoadCommands()
    {
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        foreach (var type in commandTypes)
        {
            if (Activator.CreateInstance(type) is ICommand command)
            {
                _commands[command.Name] = command;
            }
        }
    }

    private async Task HandleCommand(string command, string args)
    {
        try
        {
            if (_commands.TryGetValue(command.ToLower(), out var cmd))
            {
                await cmd.Execute(args);
            }
            else
            {
                Logger.Error($"Command {command} not found");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error handling command {command}: {ex}");
        }
    }

}
