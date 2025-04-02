using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AetherLinkServer.DalamudServices;
using AetherLinkServer.Models;
using AetherLinkServer.Utility;
using Dalamud.Plugin.Services;

namespace AetherLinkServer.Handlers;

public class CommandHandler
{
    private readonly Dictionary<string, ICommand> _commands = new();
    private Plugin plugin;

    public CommandHandler(Plugin plugin)
    {
        this.plugin = plugin;
        LoadCommands();
        Plugin.server.OnCommandReceived += HandleCommand;
    }

    private IPluginLog Logger => Svc.Log;

    private void LoadCommands()
    {
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
                                   .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        foreach (var type in commandTypes)
            if (Activator.CreateInstance(type) is ICommand command)
                _commands[command.Name] = command;
    }

    private async Task HandleCommand(string command, string args)
    {
        try
        {
            if (_commands.TryGetValue(command.ToLower(), out var cmd))
                await Svc.Framework.Run(async () => await cmd.Execute(args, plugin));
            else
            {
                await CommandHelper.SendCommandResponse("Command not found. See 'help' for available commands",
                                                        CommandResponseType.Error);
                Logger.Error($"Command {command} not found");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error handling command {command}: {ex}");
        }
    }
}
