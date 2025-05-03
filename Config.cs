using System.Collections.Generic;
using System.ComponentModel;
using PlayerRoles;

namespace RemoteKeycardLabApi;

public class Config
{
    [Description("Setting to debug the plugin")] 
    public bool Debug { get; set; } = false;
    [Description("Setting to enable/disable the plugin")] 
    public bool Enabled { get; set; } = true;
    [Description("List of stuff that the remote keycard can be used on. Toggle each one with false or true")]
    public Dictionary<string, bool> UseList { get; set; } = new()
    {
        { "Door", true },
        { "Locker", true },
        { "Generator", true },
        { "Warhead", true }
    };
    [Description("Setting to enable/disable amnesia affecting the use of the remote keycard")]
    public bool AmnesiaAffectsKeycard { get; set; } = false;
    [Description("Setting to assign roles that cannot use the remote keycard")]
    public List<RoleTypeId> BlacklistedRoles { get; set; } = new()
    {
        RoleTypeId.None
    };
}

