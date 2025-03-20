using System.Collections.Generic;
using System.Reflection;
using AetherLinkServer.Models;
using System.Linq;
using System;
using AetherLinkServer.DalamudServices;
using Dalamud.Plugin.Services;

namespace AetherLinkServer.Services;
public class CommandHandler
{
    private IPluginLog Logger => Svc.Log;
    private readonly Dictionary <string, ICommand> _commands = new();

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
            if(Activator.CreateInstance(type) is ICommand command)
            {
                _commands[command.Name] = command;
            }
        }
    }

    private void HandleCommand(string command, string args)
    {
        if(_commands.TryGetValue(command.ToLower(), out var cmd))
        {
            cmd.Execute(args);
        }
        else
        {
            Logger.Error($"Command {command} not found");
        }
    }

}
