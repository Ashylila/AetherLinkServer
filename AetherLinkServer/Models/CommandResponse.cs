using System;

namespace AetherLinkServer.Models;
public class CommandResponse
{
    public CommandResponseType Type { get; set; }
    public string Message { get; set; }
    public DateTime TimeStamp { get; set; }

}