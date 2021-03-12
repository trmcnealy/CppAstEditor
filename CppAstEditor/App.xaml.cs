using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

using Prism.DryIoc;
using Prism.Ioc;

namespace CppAstEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static string GetOSArchitecture()
        {
            switch(RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                {
                    return "x86";
                }
                case Architecture.X64:
                {
                    return "x64";
                }
                case Architecture.Arm:
                {
                    return "Arm";
                }
                case Architecture.Arm64:
                {
                    return "Arm64";
                }
            }

            throw new NotSupportedException();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static string GetOperatingSystem()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "win";
            }
            
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "linux";
            }
            
            if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "osx";
            }
            
            if(RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                return "freebsd";
            }

            throw new NotSupportedException();
        }
        
        static App()
        {
            InstallExceptionHandlers();

            string libclangRuntimesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes", $"{GetOperatingSystem()}-{GetOSArchitecture()}", "native", "libclang.dll");
            string libclangPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libclang.dll");

            if(File.Exists(libclangPath))
            {
                NativeLibrary.Load(libclangPath);
            }
            else if(File.Exists(libclangRuntimesPath))
            {
                NativeLibrary.Load(libclangRuntimesPath);
            }
        }

        static void InstallExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (s,
                                                           e) => ShowException(e.ExceptionObject as Exception);
        }

        static void ShowException(Exception? ex)
        {
            string msg = ex?.ToString() ?? "Unknown exception";
            MessageBox.Show(msg, "CppAstEditor", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private MainWindow _mainWindow;

        public const string LibraryName = "libclang";

        private IntPtr _handle;

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<MainWindowModel>();

            containerRegistry.RegisterForNavigation<MainWindow, MainWindowModel>();
        }

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);



        //    //string operatingSystem      = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : "linux";
        //    //string platformArchitecture = RuntimeInformation.ProcessArchitecture == Architecture.X64 ? "x64" : "x86";

        //    //string nativeLibraryPath = $"runtimes\\{operatingSystem}-{platformArchitecture}\\native";

        //    //Assembly assembly = typeof(CSharpConverterOptions).Assembly;

        //    //int index = assembly.Location.LastIndexOf("\\", StringComparison.Ordinal);

        //    //if(Directory.Exists(Path.Combine(assembly.Location.Substring(0, index), nativeLibraryPath)))
        //    //{
        //    //    _handle = NativeLibrary.Load(Path.Combine(nativeLibraryPath, LibraryName + ".dll"));
        //    //}
        //    //else
        //    //{
        //    //    _handle = NativeLibrary.Load(LibraryName + ".dll");
        //    //}

        //    _mainWindow = new MainWindow();
        //    _mainWindow.Show();
        //}

        //protected override void OnExit(ExitEventArgs e)
        //{
        //    base.OnExit(e);

        //    //NativeLibrary.Free(_handle);

        //    _mainWindow.UpdateAndSaveSettings();

        //    _mainWindow.Close();

        //    e.ApplicationExitCode = 0;
        //}
    }
}
