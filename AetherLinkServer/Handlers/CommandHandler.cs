using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AetherLinkServer.Attributes;
using AetherLinkServer.DalamudServices;
using AetherLinkServer.Models;
using AetherLinkServer.Services;
using AetherLinkServer.Utility;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;

namespace AetherLinkServer.Handlers;

public class CommandHandler
{
    private readonly Dictionary<string, Func<string, Task>> _commands = new();
    private readonly Plugin _plugin;
    private readonly IPluginLog _logger;
    private readonly WebSocketServer _server;
    private readonly IFramework _framework;

    public CommandHandler(Plugin plugin, WebSocketServer server, IPluginLog logger, IFramework framework)
    {
        _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _framework = framework ?? throw new ArgumentNullException(nameof(framework));
        
    }
    public void Initialize()
    {
        LoadCommands();
        _server.OnCommandReceived += HandleCommand;
        _logger.Information("CommandHandler initialized.");
    }
    
    private void LoadCommands()
    {
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
                                   .Where(t => typeof(CommandBase).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in commandTypes)
        {
            try
            {
                var instance = (CommandBase)ServiceWrapper.Get(type);
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<CommandAttribute>();
                    if (attr == null) continue;
                    var parameters = method.GetParameters();
                    if(method.ReturnType != typeof(Task) || parameters.Length != 1 || parameters[0].ParameterType != typeof(string))
                    {
                        _logger.Error($"Command {type.Name} method {method.Name} is not valid. It must return Task and take a single string argument.");
                        continue;
                    }

                    Func<string, Task> del = (string args) =>
                    {
                        if (instance is CommandBase cmdBase)
                        {
                            cmdBase.SetContext(new CommandContext()
                            {
                                Server = _server,
                                CommandName = attr.Name,
                                Description = attr.Description
                            });
                        }

                        return (Task)method.Invoke(instance, new object[] { args })!;
                    };
                    
                    _commands[attr.Name.ToLower()] = del;
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
            await _framework.Run(() => cmd(args)); // Execute the command asynchronously
        }
        else
        {
            // Handle case where the command is not found in the dictionary
            var errorMessage = $"Command '{command}' not found. See 'help' for available commands.";
            await _server.SendCommandResponse(errorMessage, CommandResponseType.Error);
            _logger.Error($"Command '{command}' not found in command dictionary.");
        }
    }
    catch (ArgumentNullException argEx)
    {
        // Specific handling for null arguments
        var errorMessage = $"Null argument exception while handling command '{command}': {argEx.Message}";
        await _server.SendCommandResponse(errorMessage, CommandResponseType.Error);
        _logger.Error(errorMessage, argEx);
    }
    catch (JsonException jsonEx)
    {
        // Specific handling for JSON deserialization issues
        var errorMessage = $"JSON deserialization error while handling command '{command}': {jsonEx.Message}";
        await _server.SendCommandResponse(errorMessage, CommandResponseType.Error);
        _logger.Error(errorMessage, jsonEx);
    }
    catch (InvalidOperationException invalidOpEx)
    {
        // Specific handling for invalid operations, such as incorrect states
        var errorMessage = $"Invalid operation error while handling command '{command}': {invalidOpEx.Message}";
        await _server.SendCommandResponse(errorMessage, CommandResponseType.Error);
        _logger.Error(errorMessage, invalidOpEx);
    }
    catch (Exception ex)
    {
        // General exception handling
        var errorMessage = $"Error handling command '{command}' with args '{args}': {ex.Message}";
        await _server.SendCommandResponse(errorMessage, CommandResponseType.Error);
        _logger.Error(errorMessage, ex);
    }
    finally
    {
        // Optionally, log that the command handling process has finished
        _logger.Debug($"Finished handling command: {command} with args: {args}");
    }
}

}

