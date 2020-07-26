using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace ResLogger
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        private bool _logToFile = false;
        public bool LogToFile {
	        get => _logToFile;
	        set {
                if (value)
					resLogger?.OpenLogStream();
                else
                    resLogger?.CloseLogStream();
                _logToFile = value;
	        }
        }


        public bool AutoScroll { get; set; } = true;

        [NonSerialized] private DalamudPluginInterface pluginInterface;

        [NonSerialized] private ResLogger resLogger;

        public void Initialize(DalamudPluginInterface pluginInterface, ResLogger resLogger)
        {
            this.pluginInterface = pluginInterface;
            this.resLogger = resLogger;
        }

        public void Save()
        {
            pluginInterface.SavePluginConfig(this);
        }
    }
}
