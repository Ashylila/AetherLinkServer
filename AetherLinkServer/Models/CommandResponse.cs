using System;

namespace AetherLinkServer.Models;
#nullable disable
public class CommandResponse
{
    public CommandResponseType Type { get; set; }
    public string Message { get; set; }
}
