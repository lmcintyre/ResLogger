using ImGuiNET;
using System;
using System.Numerics;

namespace ResLogger
{
    class PluginUI : IDisposable
    {
        private Configuration configuration;
        private ResLogger resLogger;

		private bool visible = false;
        public bool Visible
        {
            get => visible;
            set => visible = value;
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get => settingsVisible;
            set => settingsVisible = value;
        }

        public PluginUI(Configuration configuration, ResLogger resLogger)
        {
            this.configuration = configuration;
            this.resLogger = resLogger;
        }

        public void Dispose()
        {
			
        }

        public void Draw()
        {
            DrawMainWindow();
        }

        public void DrawMainWindow()
        {
	        if (!Visible) return;

			// this totally isn't copied from the dalamud log window
	        ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
	        if (ImGui.Begin("ResLogger", ref visible)) {

		        bool autoScrollConfigValue = configuration.AutoScroll;
		        bool logFileConfigValue = configuration.LogToFile;

		        if (ImGui.BeginPopup("Options"))
		        {
			        if (ImGui.Checkbox("Auto-scroll", ref autoScrollConfigValue))
			        {
				        configuration.AutoScroll = autoScrollConfigValue;
				        configuration.Save();
			        }

			        if (ImGui.Checkbox("Log to File", ref logFileConfigValue)) {
				        configuration.LogToFile = logFileConfigValue;
						configuration.Save();
			        }
                    ImGui.EndPopup();
		        }

		        if (ImGui.Button("Options"))
			        ImGui.OpenPopup("Options");
		        ImGui.SameLine();
		        var clear = ImGui.Button("Clear");
		        ImGui.SameLine();
		        var copy = ImGui.Button("Copy");

		        if (clear)
			        resLogger.Paths.Clear();

		        if (copy)
			        ImGui.SetClipboardText(resLogger.GetClipText());

                ImGui.BeginChild("scrolling", new Vector2(0, 0), false, ImGuiWindowFlags.HorizontalScrollbar);
                
		        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
		        foreach (string str in resLogger.PathsArr)
			        ImGui.Text(str);
		        ImGui.PopStyleVar();

		        if (configuration.AutoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
			        ImGui.SetScrollHereY(1.0f);

		        ImGui.EndChild();
	        }

	        if (!Visible)
				resLogger.Paths.Clear();

	        ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            
        }
    }
}
