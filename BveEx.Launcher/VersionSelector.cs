﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;

using BveEx.Launcher.Hosting;
using BveEx.Launcher.SplashScreen;

namespace BveEx.Launcher
{
    public class VersionSelector
    {
        private static readonly TargetBveFinder BveFinder = new TargetBveFinder();

        static VersionSelector()
        {
#if DEBUG
            if (!Debugger.IsAttached) Debugger.Launch();
#endif

            IpcClientChannel channel = new IpcClientChannel();
            ChannelServices.RegisterChannel(channel, true);
        }

        private readonly Process SplashProcess;
        private readonly SplashFormInfo SplashForm;

        public ICoreHost CoreHost { get; }

        public VersionSelector(Assembly callerAssembly)
        {
            Assembly launcherAssembly = Assembly.GetExecutingAssembly();
            string rootDirectory = Path.GetDirectoryName(launcherAssembly.Location);

            string legacyFilePath = Path.Combine(rootDirectory, ".LEGACY");
            bool isLegacyMode = File.Exists(legacyFilePath);
            string productName = isLegacyMode ? "BveEX レガシーモード (AtsEX)" : "BveEX";

            Version bveVersion = BveFinder.TargetAssembly.GetName().Version;
            Version launcherVersion = launcherAssembly.GetName().Version;
            Guid channelGuid = Guid.NewGuid();
            SplashProcess = Process.Start(Path.Combine(rootDirectory, "BveEx.Launcher.SplashScreen.exe"), $"{bveVersion} {launcherVersion} {channelGuid}");
            while (SplashProcess.MainWindowHandle == IntPtr.Zero)
            {
                Task.Delay(10).Wait();
                SplashProcess.Refresh();
            }

            SplashForm = (SplashFormInfo)Activator.GetObject(typeof(SplashFormInfo), $"ipc://{channelGuid}/{nameof(SplashFormInfo)}");
            SplashForm.ProgressText = $"{productName} を探しています...";

            string exAssemblyDirectory;
            if (isLegacyMode)
            {
                exAssemblyDirectory = Path.Combine(rootDirectory, "Legacy");
            }
            else
            {
#if DEBUG
                exAssemblyDirectory = Path.Combine(rootDirectory, "Debug");
#else
                IEnumerable<string> availableDirectories = Directory.GetDirectories(rootDirectory).Where(x => x.Contains('.'));
                IEnumerable<(string Directory, AssemblyName AssemblyName)> bveExAssemblies = availableDirectories
                    .Select(x => (Directory: x, Location: Path.Combine(x, "BveEx.dll")))
                    .Where(x => File.Exists(x.Location))
                    .Select(x => (x.Directory, AssemblyName: AssemblyName.GetAssemblyName(x.Location)))
                    .OrderBy(x => x.AssemblyName.Version);

                if (!bveExAssemblies.Any())
                {
                    ErrorDialog.Show(4, $"BveEX 本体の読込に失敗しました。候補となる BveEX 本体フォルダが見つかりませんでした。",
                        "BveEX を再インストールしてください。");
                    throw new NotSupportedException();
                }

                exAssemblyDirectory = bveExAssemblies.Last().Directory; // TODO: バージョンを選択できるようにする
#endif
            }

            if (!Directory.Exists(exAssemblyDirectory))
            {
                ErrorDialog.Show(3, $"{productName} 本体の読込に失敗しました。フォルダ '{exAssemblyDirectory}' が見つかりませんでした。",
                    "BveEX を再インストールしてください。");
                throw new NotSupportedException();
            }

            if (!isLegacyMode)
            {
                SplashForm.ProgressText = "アップデートを確認しています...";

                if (ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls) || ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls11))
                {
                    ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls;
                    ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls11;
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                }

                UpdateChecker.CheckUpdates();
            }

            SplashForm.ProgressText = $"{productName} を起動しています...";

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            try
            {
                CoreHost = isLegacyMode ? (ICoreHost)new LegacyCoreHost(callerAssembly, BveFinder) : new CoreHost(callerAssembly, BveFinder);
            }
            finally
            {
                if (!SplashProcess.HasExited) SplashProcess.Kill();
            }


            Assembly AssemblyResolve(object sender, ResolveEventArgs e)
            {
                AssemblyName assemblyName = new AssemblyName(e.Name);
                switch (assemblyName.Name)
                {
                    case "AtsEx.Diagnostics":
                    case "BveEx.Diagnostics":
                        string diagnosticsPath = Path.Combine(rootDirectory, assemblyName.Name + ".dll");
                        return Assembly.LoadFrom(diagnosticsPath);

                    default:
                        string path = Path.Combine(exAssemblyDirectory, assemblyName.Name + ".dll");
                        return File.Exists(path) ? Assembly.LoadFrom(path) : null;
                }
            }
        }
    }
}
