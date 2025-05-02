using System;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;

namespace RemoteKeycardLabApi
{
    public class RemoteKeycard : Plugin<Config>
    {
        public static RemoteKeycard Instance { get; private set; } 
        public Events Events { get; } = new();
        public override string Name { get; } = "RemoteKeycard";
        public override string Description { get; } = "Allows you to open doors/lockers and generators withouth holding a card";
        public override string Author { get; } = "sssssssthedev";
        public override Version Version { get; } = new Version(1, 0, 2);
        public override Version RequiredApiVersion { get; } = new (LabApiProperties.CompiledVersion);
        public override LoadPriority Priority { get; } = LoadPriority.High;
        public override string ConfigFileName { get; set; } = "remotekeycard.yml";
        
        public override void Enable()
        {
            if (!Config.Enabled)
            {
                Logger.Info("Plugin is set to not start, change the configuration file if this is a mistake");
                return;
            }
            Logger.Info("Starting plugin...");
            Instance = this;
            Logger.Info("Registering events...");
            CustomHandlersManager.RegisterEventsHandler(Events);
            Logger.Info("Plugin has been enabled.");
        }

        public override void Disable()
        {
            Logger.Info("Stopping plugin...");
            Instance = null;
            Logger.Info("Unregistering events...");
            CustomHandlersManager.UnregisterEventsHandler(Events);
            Logger.Info("Plugin has been disabled.");
        }
    }
}