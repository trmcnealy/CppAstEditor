using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace CppAstEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private MainWindow _mainWindow;

        public const string LibraryName = "libclang";

        protected override void OnStartup(StartupEventArgs e)
        {
            string operatingSystem      = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : "linux";
            string platformArchitecture = RuntimeInformation.ProcessArchitecture == Architecture.X64 ? "x64" : "x86";

            string nativeLibraryPath = $"runtimes\\{operatingSystem}-{platformArchitecture}\\native";

            IntPtr Handle = NativeLibrary.Load(Path.Combine(nativeLibraryPath, LibraryName + ".dll"));

            _mainWindow = new MainWindow();
            _mainWindow.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _mainWindow.UpdateAndSaveSettings();

            e.ApplicationExitCode = 0;
        }
    }
}
