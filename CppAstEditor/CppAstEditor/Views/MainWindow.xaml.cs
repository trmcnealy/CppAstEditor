
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CppAstEditor
{
    public partial class MainWindow
    {
        private readonly MainWindowModel WindowModel = new MainWindowModel();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = WindowModel;

            WindowModel.Timer.Tick    += Timer_Tick;
            WindowModel.JitTimer.Tick += JitTimer_Tick;
        }

        private void Window_Closing(object?         sender,
                                    CancelEventArgs e)
        {
            Console.WriteLine("Window_Closing.");

            WindowModel.Dispose();
        }

        private void DefinesAddButton_OnClick(object?         sender,
                                              RoutedEventArgs e)
        {
            WindowModel.Defines.Add(string.Empty);

            e.Handled = true;
        }

        private void DefinesRemoveButton_OnClick(object?         sender,
                                                 RoutedEventArgs e)
        {
            if(WindowModel.SelectedDefinesIndex >= 0 && WindowModel.SelectedDefinesIndex < WindowModel.Defines.Count)
            {
                WindowModel.Defines.RemoveAt(WindowModel.SelectedDefinesIndex);
            }

            e.Handled = true;
        }

        private void CppTextBox_TextChanged(object?   sender,
                                            EventArgs e)
        {
            WindowModel.Timer.Stop();
            WindowModel.Timer.Start();
        }

        private void ImportSettingsCommand_Executed(object?         sender,
                                                    RoutedEventArgs e)
        {
            WindowModel.ImportSettings();
            //Initialize();
        }

        private void ExportSettingsCommand_Executed(object?         sender,
                                                    RoutedEventArgs e)
        {
            WindowModel.ExportSettings();
        }

        private void CSharpText_DocumentChanged(object?   sender,
                                                EventArgs e)
        {
        }

        private void DefinesTextBox_TextChanged(object?              sender,
                                                TextChangedEventArgs e)
        {
            if(sender is EditableTextBlock textBox && WindowModel.SelectedDefinesIndex >= 0 && WindowModel.SelectedDefinesIndex < WindowModel.Defines.Count)
            {
                WindowModel.Defines[WindowModel.SelectedDefinesIndex] = textBox.Text;
            }

            e.Handled = true;
        }

        private void AdditionalArgumentsAddButton_OnClick(object?         sender,
                                                          RoutedEventArgs e)
        {
            WindowModel.AdditionalArguments.Add(string.Empty);

            e.Handled = true;
        }

        private void AdditionalArgumentsRemoveButton_OnClick(object?         sender,
                                                             RoutedEventArgs e)
        {
            if(WindowModel.SelectedAdditionalArgumentsIndex >= 0 && WindowModel.SelectedAdditionalArgumentsIndex < WindowModel.AdditionalArguments.Count)
            {
                WindowModel.AdditionalArguments.RemoveAt(WindowModel.SelectedAdditionalArgumentsIndex);
            }

            e.Handled = true;
        }

        private void IncludeFoldersAddButton_OnClick(object?         sender,
                                                     RoutedEventArgs e)
        {
            WindowModel.IncludeFolders.Add(string.Empty);

            e.Handled = true;
        }

        private void IncludeFoldersRemoveButton_OnClick(object?         sender,
                                                        RoutedEventArgs e)
        {
            if(WindowModel.SelectedIncludeFoldersIndex >= 0 && WindowModel.SelectedIncludeFoldersIndex < WindowModel.IncludeFolders.Count)
            {
                WindowModel.IncludeFolders.RemoveAt(WindowModel.SelectedIncludeFoldersIndex);
            }

            e.Handled = true;
        }

        private void CppTextBox_DocumentChanged(object?   sender,
                                                EventArgs e)
        {
            WindowModel.Timer.Stop();
            WindowModel.Timer.Start();
        }

        private void AdditionalArgumentsTextBox_TextChanged(object?              sender,
                                                            TextChangedEventArgs e)
        {
            if(sender is EditableTextBlock textBox               &&
               WindowModel.SelectedAdditionalArgumentsIndex >= 0 &&
               WindowModel.SelectedAdditionalArgumentsIndex < WindowModel.AdditionalArguments.Count)
            {
                WindowModel.AdditionalArguments[WindowModel.SelectedAdditionalArgumentsIndex] = textBox.Text;
            }

            e.Handled = true;
        }

        private void IncludeFoldersTextBox_TextChanged(object?              sender,
                                                       TextChangedEventArgs e)
        {
            if(sender is EditableTextBlock textBox && WindowModel.SelectedIncludeFoldersIndex >= 0 && WindowModel.SelectedIncludeFoldersIndex < WindowModel.IncludeFolders.Count)
            {
                WindowModel.IncludeFolders[WindowModel.SelectedIncludeFoldersIndex] = textBox.Text;
            }

            e.Handled = true;
        }

        private void SystemIncludeFoldersTextBox_TextChanged(object?              sender,
                                                             TextChangedEventArgs e)
        {
            if(sender is EditableTextBlock textBox                &&
               WindowModel.SelectedSystemIncludeFoldersIndex >= 0 &&
               WindowModel.SelectedSystemIncludeFoldersIndex < WindowModel.SystemIncludeFolders.Count)
            {
                WindowModel.SystemIncludeFolders[WindowModel.SelectedSystemIncludeFoldersIndex] = textBox.Text;
            }

            e.Handled = true;
        }

        private void SystemIncludeFoldersAddButton_OnClick(object?         sender,
                                                           RoutedEventArgs e)
        {
            WindowModel.SystemIncludeFolders.Add(string.Empty);

            e.Handled = true;
        }

        public void UpdateAndSaveSettings()
        {
            WindowModel.UpdateAndSaveSettings();
        }
        
        private void SystemIncludeFoldersRemoveButton_OnClick(object?         sender,
                                                              RoutedEventArgs e)
        {
            if(WindowModel.SelectedSystemIncludeFoldersIndex >= 0 && WindowModel.SelectedSystemIncludeFoldersIndex < WindowModel.SystemIncludeFolders.Count)
            {
                WindowModel.SystemIncludeFolders.RemoveAt(WindowModel.SelectedSystemIncludeFoldersIndex);
            }

            e.Handled = true;
        }

        private void Timer_Tick(object?   sender,
                                EventArgs e)
        {
            WindowModel.Timer.Stop();

            if(WindowModel.CppText.TextLength > 0)
            {
                WindowModel.ConverterOptions.Defines.Clear();
                WindowModel.ConverterOptions.Defines.AddRange(WindowModel.Defines);
                WindowModel.ConverterOptions.AdditionalArguments.Clear();
                WindowModel.ConverterOptions.AdditionalArguments.AddRange(WindowModel.AdditionalArguments);
                WindowModel.ConverterOptions.IncludeFolders.Clear();
                WindowModel.ConverterOptions.IncludeFolders.AddRange(WindowModel.IncludeFolders);
                WindowModel.ConverterOptions.SystemIncludeFolders.Clear();
                WindowModel.ConverterOptions.SystemIncludeFolders.AddRange(WindowModel.SystemIncludeFolders);

                WindowModel.CSharpText.Text = MainWindowModel.ComplieCode(WindowModel.CppText.Text, WindowModel.ConverterOptions);
            }
        }

        private void JitTimer_Tick(object?   sender,
                                   EventArgs e)
        {
            WindowModel.JitTimer.Stop();

            if(WindowModel.CsJitText.TextLength > 0)
            {
                AsmJitTextBox.Text = MainWindowModel.ComplieCs(CsJitTextBox.Text);
            }
        }

        private void CsJitTextBox_DocumentChanged(object?   sender,
                                                  EventArgs e)
        {
            WindowModel.JitTimer.Stop();
            WindowModel.JitTimer.Start();
        }

        private void CsJitTextBox_TextChanged(object?   sender,
                                              EventArgs e)
        {
            WindowModel.JitTimer.Stop();
            WindowModel.JitTimer.Start();
        }

        private void AsmJitTextBox_DocumentChanged(object?   sender,
                                                   EventArgs e)
        {
        }
    }
}