using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

using CppAst.CodeGen.CSharp;

namespace CppAstEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        static App()
        {
            InstallExceptionHandlers();
        }

        static void InstallExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (s,
                                                           e) => ShowException(e.ExceptionObject as Exception);
        }

        static void ShowException(Exception ex)
        {
            string msg = ex?.ToString() ?? "Unknown exception";
            MessageBox.Show(msg, "CppAstEditor", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private MainWindow _mainWindow;

        public const string LibraryName = "libclang";

        private IntPtr _handle;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);



            //string operatingSystem      = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : "linux";
            //string platformArchitecture = RuntimeInformation.ProcessArchitecture == Architecture.X64 ? "x64" : "x86";

            //string nativeLibraryPath = $"runtimes\\{operatingSystem}-{platformArchitecture}\\native";

            //Assembly assembly = typeof(CSharpConverterOptions).Assembly;

            //int index = assembly.Location.LastIndexOf("\\", StringComparison.Ordinal);

            //if(Directory.Exists(Path.Combine(assembly.Location.Substring(0, index), nativeLibraryPath)))
            //{
            //    _handle = NativeLibrary.Load(Path.Combine(nativeLibraryPath, LibraryName + ".dll"));
            //}
            //else
            //{
            //    _handle = NativeLibrary.Load(LibraryName + ".dll");
            //}

            _mainWindow = new MainWindow();
            _mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            NativeLibrary.Free(_handle);

            _mainWindow.UpdateAndSaveSettings();

            _mainWindow.Close();

            e.ApplicationExitCode = 0;
        }
    }
}
