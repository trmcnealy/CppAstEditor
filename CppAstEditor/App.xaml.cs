using System.Windows;

namespace CppAstEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private MainWindow _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
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
