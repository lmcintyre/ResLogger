using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Hooking;

namespace ResLogger
{
	public class ResLogger : IDalamudPlugin {

        // these totally aren't copied from Dalamud.Game.Internal.File.Resource stuff
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		private delegate IntPtr GetResourceAsyncDelegate(IntPtr manager, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, byte a7);
		private Hook<GetResourceAsyncDelegate> getResourceAsyncHook;

		// [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		// private delegate IntPtr GetResourceSyncDelegate(IntPtr manager, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6);
		// private Hook<GetResourceSyncDelegate> getResourceSyncHook;

	    public string Name => "Resource Logger";
	    private const string CommandName = "/reslog";
	    private string FileName { get; set; }
	    
        private DalamudPluginInterface pi;
        private Configuration configuration;
        private PluginUI ui;
        private FileStream logStream;

        public List<string> Paths { get; private set; }
        private List<string> WritePaths { get; set; }
        public string[] PathsArr => Paths.ToArray();

        public void Initialize(DalamudPluginInterface pluginInterface) {
            pi = pluginInterface;

            configuration = pi.GetPluginConfig() as Configuration ?? new Configuration();
            configuration.Initialize(pi, this);
            ui = new PluginUI(configuration, this);
            
            pi.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
	            { HelpMessage = "Display the resource log."});

            Paths = new List<string>();
            WritePaths = new List<string>();
            FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "reslog.txt");

            if (configuration.LogToFile)
                OpenLogStream();

            // neither are these
            IntPtr getResourceAsync = pi.TargetModuleScanner
	            .ScanText("48 89 5C 24 08 48 89 54  24 10 57 48 83 EC 20 B8 03 00 00 00 48 8B F9 86  82 A1 00 00 00 48 8B 5C 24 38 B8 01 00 00 00 87  83 90 00 00 00 85 C0 74");
            // IntPtr getResourceSync = pi.TargetModuleScanner.
	           //  ScanText("48 89 5C 24 08 48 89 6C  24 10 48 89 74 24 18 57 41 54 41 55 41 56 41 57  48 83 EC 30 48 8B F9 49 8B E9 48 83 C1 30 4D 8B  F0 4C 8B EA FF 15 CE F6");

            getResourceAsyncHook = new Hook<GetResourceAsyncDelegate>(getResourceAsync, new GetResourceAsyncDelegate(GetResourceAsyncDetour));
            // getResourceSyncHook = new Hook<GetResourceSyncDelegate>(getResourceSync, new GetResourceSyncDelegate(GetResourceSyncDetour));

            getResourceAsyncHook.Enable();
            // getResourceSyncHook.Enable();
            
            pi.UiBuilder.OnBuildUi += DrawUI;
            pi.UiBuilder.OnOpenConfigUi += (sender, args) => DrawConfigUI();
        }

        public void Dispose() {
			
            if (logStream != null)
				CloseLogStream();
            WritePaths.Clear();
            Paths.Clear();
            WritePaths = null;
            Paths = null;

            getResourceAsyncHook.Disable();
            getResourceAsyncHook.Dispose();
            
            // getResourceSyncHook.Disable();
            // getResourceSyncHook.Dispose();

            ui.Dispose();
            pi.CommandManager.RemoveHandler(CommandName);
            pi.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            ui.Visible = true;
        }

        private void DrawUI()
        {
            ui.Draw();
        }

        private void DrawConfigUI()
        {
            ui.SettingsVisible = true;
        }

        public void OpenLogStream() {
	        logStream = File.Open(FileName, FileMode.Append);
        }

        public void CloseLogStream() {
			FlushLogStream();
            logStream.Close();
        }

        public void FlushLogStream() {
	        if (logStream == null) return;
	        StringBuilder sb = new StringBuilder();
	        foreach (string str in WritePaths.ToArray()) {
		        sb.Append(str);
		        sb.Append("\n");
	        }
	        byte[] bytes = Encoding.ASCII.GetBytes(sb.ToString());
	        logStream.Write(bytes, 0, bytes.Length);
	        logStream.Flush();
            WritePaths.Clear();
        }

        private IntPtr GetResourceAsyncDetour(IntPtr manager, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, byte a7)
        {
	        var path = Marshal.PtrToStringAnsi(a5);

            // may have been garbage in the reg
            if (!IsAscii(path)) return getResourceAsyncHook.Original(manager, a2, a3, a4, a5, a6, a7);

            if (ui.Visible) 
				Paths.Add(path);

            if (configuration.LogToFile)
	            WritePaths.Add(path);

            if (WritePaths.Count > 100)
	            FlushLogStream();

            return getResourceAsyncHook.Original(manager, a2, a3, a4, a5, a6, a7);
        }

        public static bool IsAscii(string value)
        {
	        return Encoding.UTF8.GetByteCount(value) == value.Length;
        }

        // private IntPtr GetResourceSyncDetour(IntPtr manager, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6)
        // {
        // 	var path = Marshal.PtrToStringAnsi(a5);
        //
        // 	if (ui.Visible)
        // 		Paths.Add(path);
        //
        // 	if (configuration.ShouldLogToDalamudLog)
        // 		PluginLog.Log(path);
        //
        // 	return getResourceSyncHook.Original(manager, a2, a3, a4, a5, a6);
        // }

        public string GetClipText() {
	        StringBuilder bldr = new StringBuilder();
	        foreach (string str in PathsArr) {
		        bldr.Append(str);
		        bldr.Append("\n");
	        }
	        return bldr.ToString();
        }
	}
}