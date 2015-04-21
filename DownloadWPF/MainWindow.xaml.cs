using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DownloadWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string Filename;
        Client.TestClient tc;
        string foldername;
        public MainWindow()
        {
            InitializeComponent();
            string url = "http://localhost:55664/FileService/api/File";
            tc = new Client.TestClient(url);
            foldername = "";
            Filename = "";
           // tabs.Items.Remove(RenameTab);
            Directory_Load();
            fileInfo.AutoGeneratingColumn += fileInfoColumn_Load;
        }

        private void Directory_Load()
        {
            var directory = new ObservableCollection<DirRecord>();

            directory.Add(
                new DirRecord
                {
                    Info=new DirectoryInfo("../../../RoleBase/Uploads/")
                }
                );
            directoryTreeView.ItemsSource = directory;
        }

        private void fileInfoColumn_Load(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            List<string> requiredProperties = new List<string>
            {
                "Name", "Length",  "Extension","LastWriteTime"
            };

            if (!requiredProperties.Contains(e.PropertyName))
            {
                e.Cancel = true;
            }
            
            else
            {
                e.Column.Header = e.Column.Header.ToString();
            }
        }


        private void Download(object sender, RoutedEventArgs e)
        {
            Task task = downloadFiles(Filename);
        }

        private void DownloadWithDepen(object sender, RoutedEventArgs e)
        {
            Task task = downloadDepen(Filename);
        }

        async private Task downloadDepen(string filename)
        {
            List<string> fileset = tc.GetDependencies(filename);
            fileset.Add(filename);
            foreach (string f in fileset)
            {
                if(f.LastIndexOf("\\")!=f.Length-1)
                await Task.Run(() => tc.downLoadFile(f));
                else
                await Task.Run(() => tc.downLoadFolder(f));
            }
            string messageBoxText = "Download Completed!";
            string caption = "Download Result";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
        }
        async private Task downloadFiles(string filename)
        {

            await Task.Run(() => tc.downLoadFile(filename));            
            string messageBoxText = "Download Completed!";
            string caption = "Download Result";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
            
        }

       private void DeleteFolder(){
           string clientPath = foldername.Substring(foldername.IndexOf("Uploads") + 7);
           string DownloadPath = System.IO.Path.GetFullPath("../../DownLoad");
           DirectoryInfo download = new DirectoryInfo(DownloadPath);
           if (!download.Exists) Directory.CreateDirectory(DownloadPath);
           clientPath = DownloadPath+clientPath;
           DirectoryInfo downfolder=new DirectoryInfo(clientPath);
           if(downfolder.Exists)
           Directory.Delete(clientPath, true);
        }

        async private Task SyncFolder()
        {
            DeleteFolder();
            
            DirectoryInfo dir=new DirectoryInfo(foldername);
            await Task.Run(() => tc.downLoadFolder(foldername));
            foreach (DirectoryInfo d in dir.GetDirectories("*", SearchOption.AllDirectories))
            {
                await Task.Run(() => tc.downLoadFolder(d.FullName));
            }
            
            string messageBoxText = "Download Completed!";
            string caption = "Download Result";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
        }
        private void SyncDir(object sender, RoutedEventArgs e)
        {
            Task task = SyncFolder();
        }

        private void ShowMenu(object sender, RoutedEventArgs e)
        {
            object temp;           
            temp=fileInfo.SelectedItem;
            if (temp == null) return;          
            var tmp = temp as FileInfo;
            Filename = tmp.FullName;
            System.Windows.Controls.ContextMenu menu = fileInfo.FindResource("downloadMenu") as System.Windows.Controls.ContextMenu;
            menu.PlacementTarget = sender as System.Windows.Controls.Button;
            menu.IsOpen = true;
        }

        private void ShowSyncMenu(object sender, RoutedEventArgs e)
        {
            object temp;
            temp = directoryTreeView.SelectedItem;
            if (temp == null) return;
            var tmp = temp as DirRecord;
            foldername = tmp.Info.FullName;
            System.Windows.Controls.ContextMenu menu = directoryTreeView.FindResource("SyncMenu") as System.Windows.Controls.ContextMenu;
            menu.PlacementTarget = sender as System.Windows.Controls.Button;
            menu.IsOpen = true;
        }
            
        
    }

 }

