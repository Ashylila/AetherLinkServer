using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AetherLinkServer.DalamudServices;
using AetherLinkServer.Models;
using AetherLinkServer.Services;
using AetherLinkServer.Utility;
using Dalamud.Plugin.Services;

namespace AetherLinkServer.Handlers;

public class CommandHandler
{
    private readonly Dictionary<string, CommandBase> _commands = new();
    private readonly Plugin _plugin;
    private readonly IPluginLog _logger;
    private readonly WebSocketServer _server;

    public CommandHandler(Plugin plugin, WebSocketServer server, IPluginLog logger)
    {
        _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        LoadCommands();
        _server.OnCommandReceived += HandleCommand;
    }

    
    private void LoadCommands()
    {
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
                                   .Where(t => typeof(CommandBase).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in commandTypes)
        {
            try
            {
                
                if (ServiceWrapper.Get(type) is CommandBase command)
                {
                    command.SetContext(new CommandContext()
                    {
                        Server = _server,
                        CommandName = command.Name,
                        Description = command.Description
                    });
                    _commands[command.Name.ToLower()] = command;
                    _logger.Debug($"Command {command.Name} loaded");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load command {type.Name}: {ex.Message}");
            }
        }
    }

    
    private async Task HandleCommand(string command, string args)
    {
        try
        {
            
            if (_commands.TryGetValue(command.ToLower(), out var cmd))
            {
                _logger.Debug($"Executing command: {command} with args: {args}");
                await cmd.Execute(args);
            }
            else
            {
                var errorMessage = $"Command '{command}' not found. See 'help' for available commands.";
                await _server.SendCommandResponse(errorMessage, CommandResponseType.Error);
                _logger.Error($"Command {command} not found");
            }
        }
        catch (Exception ex)
        {
            // More fine-grained error handling could be added here
            var errorMessage = $"Error handling command {command}: {ex.Message}";
            await _server.SendCommandResponse(errorMessage, CommandResponseType.Error);
            _logger.Error(errorMessage);
        }
    }
}
//TODO: Implement command attribute

