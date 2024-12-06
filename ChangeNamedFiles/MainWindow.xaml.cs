using ChangeNamedFiles.Util;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using IOPath = System.IO.Path;

namespace ChangeNamedFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string DefaultPath = $"D:\\Entertaiment\\Pics";

        public MainWindow()
        {
            InitializeComponent();
            txtFolder.Text = DefaultPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFolder_Click(object sender, RoutedEventArgs e)
        {
            ///open folder
            using (CommonOpenFileDialog openFolder = new CommonOpenFileDialog())
            {
                openFolder.Title = "Select a folder";
                openFolder.IsFolderPicker = true;
                openFolder.InitialDirectory = txtFolder.Text;

                if (openFolder.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string selectedPath = openFolder.FileName;
                    txtFolder.Text = selectedPath;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnRename_Click(object sender, RoutedEventArgs e)
        {
            btnRename.IsEnabled = false;
            txtLogs.Text = string.Empty;
            var directory = txtFolder.Text;
            try
            {
                // Run the long process asynchronously
                await Task.Run(() =>
                {
                    /// get all folder
                    var folders = Directory.GetDirectories(directory);

                    foreach (var folder in folders)
                    {
                        Common.LogsInfo($"Update File in {folder}", false, UpdateLogs);

                        string name = UpdateNamedFolder(folder);

                        RenameFiles(name, folder, folder);

                        HandleSubfolder(name, folder);
                    }

                });
            }
            finally
            {
                btnRename.IsEnabled = true;
            }
        }


        /// <summary>
        /// invoke for update logs
        /// </summary>
        /// <param name="run"></param>
        public void UpdateLogs(string msg, bool isError)
        {
            Run async = null;
            Dispatcher.Invoke(() =>
            {
                async = new Run(msg);
                if (isError)
                {
                    async.Foreground = new SolidColorBrush(Colors.Red);
                }
            });

            Dispatcher.Invoke(() =>
            {
                txtLogs.Inlines.Add(async);
                txtLogs.Inlines.Add(new LineBreak());
                scrollLogs.ScrollToBottom();
            });
        }

        /// <summary>
        /// update file
        /// </summary>
        /// <param name="name"></param>
        /// <param name="folder"></param>
        /// <param name="rootFolder"></param>
        private void RenameFiles(string name, string folder, string rootFolder)
        {
            try
            {
                RenamedFilesBygroup(name, folder, rootFolder);

                HandleSubfolder(name, folder);

            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Access denied: " + e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine("Directory not found: " + e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("I/O error: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
        }


        private void RenamedFilesBygroup(string name, string folder, string rootFolder)
        {
            var groupFiles = Directory.GetFiles(folder).GroupBy(f => IOPath.GetExtension(f).ToLower()).OrderBy(g => g.Key);
            foreach (var group in groupFiles)
            {
                var cnt = 1;
                foreach (var file in group)
                {
                    var ext = IOPath.GetExtension(file);
                    var newFileName = $"{name}_({cnt}){ext}";
                    var newPath = IOPath.Combine(rootFolder, newFileName);

                    if (File.Exists(newPath))
                    {
                        Common.LogsInfo($"{newPath} already existed.", true, UpdateLogs);
                    }
                    else
                    {
                        File.Move(file, newPath);
                    }
                    cnt++;
                }
            }
        }

        private void HandleSubfolder(string name, string folder)
        {
            var subFolders = Directory.GetDirectories(folder);
            foreach (var subFolder in subFolders)
            {
                var newName = $"{name}_{IOPath.GetFileName(subFolder)}".Trim();
                RenameFiles(newName, subFolder, folder);
            }
        }

        private string UpdateNamedFolder(string folder)
        {
            string name = IOPath.GetFileName(folder);
            string pattern = @"\[.+\]";

            // Remove the matched text from the string
            return Regex.Replace(name, pattern, string.Empty).Trim();
        }

    }
}
