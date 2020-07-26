using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace UIDev
{
    class UITest : IPluginUIMock
    {
        public static void Main(string[] args)
        {
            UIBootstrap.Inititalize(new UITest());
        }

        private SimpleImGuiScene scene;

        Vector4 testPhys = new Vector4(1, 0, 0, 1);
        Vector4 testMag = new Vector4(0, 0, 1, 1);
        Vector4 testDark = new Vector4(1, 0, 1, 1);

        public void Initialize(SimpleImGuiScene scene)
        {
            // scene is a little different from what you have access to in dalamud
            // but it can accomplish the same things, and is really only used for initial setup here

            // eg, to load an image resource for use with ImGui 

            scene.OnBuildUI += Draw;

            this.Visible = true;

            // saving this only so we can kill the test application by closing the window
            // (instead of just by hitting escape)
            this.scene = scene;
        }

        public void Dispose()
        {

        }

        // You COULD go all out here and make your UI generic and work on interfaces etc, and then
        // mock dependencies and conceivably use exactly the same class in this testbed and the actual plugin
        // That is, however, a bit excessive in general - it could easily be done for this sample, but I
        // don't want to imply that is easy or the best way to go usually, so it's not done here either
        private void Draw()
        {
            DrawMainWindow();
            DrawSettingsWindow();
            
            if (!Visible)
            {
                this.scene.ShouldQuit = true;
            }
        }

        #region Nearly a copy/paste of PluginUI
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

        // this is where you'd have to start mocking objects if you really want to match
        // but for simple UI creation purposes, just hardcoding values works
        private bool fakeConfigBool = true;

        public void DrawMainWindow()
        {

	        if (!Visible)
            {
                return;
            }

	        var Paths = new List<string>();
            Paths.Add("hey");
            Paths.Add("hey test");
            Paths.Add("hey tesitng 2");
            Paths.Add("hey");
            Paths.Add("hey weeeeee");
            Paths.Add("hey");
            Paths.Add("hey aaafegrhtjt");

            // this totally isn't copied from the dalamud log window
            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("ResLogger", ref visible))
            {

	            if (ImGui.BeginPopup("Options"))
	            {
		            if (ImGui.Checkbox("Auto-scroll", ref fakeConfigBool))
		            {
		            }

		            if (ImGui.Checkbox("Log to Dalamud/logfile (spammy)", ref fakeConfigBool))
		            {

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
                    Paths.Clear();
                if (copy)
					ImGui.SetClipboardText(Paths[0]);
                
	            ImGui.BeginChild("scrolling", new Vector2(0, 0), false, ImGuiWindowFlags.HorizontalScrollbar);

	            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
	            foreach (string str in Paths)
		            ImGui.Text(str);
	            ImGui.PopStyleVar();

	            if (fakeConfigBool && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
		            ImGui.SetScrollHereY(1.0f);

	            ImGui.EndChild();
            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
        }
        #endregion
    }
}
