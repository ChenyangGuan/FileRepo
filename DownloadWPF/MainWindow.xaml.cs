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
            string url = "http://localhost:55664/api/File";
            tc = new Client.TestClient(url);
            foldername = "";
            Filename = "";

            RefreshTabs();
        }
        private void RefreshTabs(){
            Directory_Load();
            fileInfo.AutoGeneratingColumn += fileInfoColumn_Load;
            ClientfileInfo.AutoGeneratingColumn += fileInfoColumn_Load;
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
            var Clientdirectory = new ObservableCollection<DirRecord>();

            Clientdirectory.Add(
                new DirRecord
                {
                    Info = new DirectoryInfo("../../DownLoad/")
                }
                );
            ClientdirectoryTreeView.ItemsSource = Clientdirectory;
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

        //Download Dependencies
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
            RefreshTabs();
        }
        async private Task downloadFiles(string filename)
        {

            await Task.Run(() => tc.downLoadFile(filename));            
            string messageBoxText = "Download Completed!";
            string caption = "Download Result";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
            RefreshTabs();
            
        }

        //Delete files existing in client but not in Server
        private void DeleteFiles(ref List<string>d)
        {
            foreach (string path in d)
            {
                File.Delete(path);
            }
            string DownloadPath = System.IO.Path.GetFullPath("../../DownLoad");
            DirectoryInfo dir = new DirectoryInfo(DownloadPath);
            foreach (DirectoryInfo folder in dir.GetDirectories("*", SearchOption.AllDirectories))
            {
                if (folder.GetFiles() == null && folder.GetDirectories() == null) Directory.Delete(folder.FullName);
            }
        }

     
        //Change the Absolute path to Relative path
       private string TurnPath(string path)
       {
           if (path.IndexOf("Uploads") != -1)
           {
               return path.Substring(path.IndexOf("Uploads") + 7, path.Length - path.IndexOf("Uploads") - 7);
           }
           else if (path.IndexOf("DownLoad") != -1)
           {
               return path.Substring(path.IndexOf("DownLoad") + 8, path.Length - path.IndexOf("DownLoad") - 8);
           }
           else return "";
       }

        // Check Whether two files are same
       private void CheckFileSame(string f1, string f2, ref List<string> a)
       {
         
               FileInfo Server = new FileInfo(f1);
               FileInfo Client = new FileInfo(f2);
               if (Server.Length != Client.Length || Server.Name != Client.Name) a.Add(f1);
           
       }

       //Compare Server and Client files
       private void CheckDiff(ref List<string> a, ref List<string> d)
       {
           SortedDictionary<string, string> Servermap = new SortedDictionary<string, string>();
           List<string> Clientset = new List<string>();
           DirectoryInfo dir = new DirectoryInfo(foldername);
           foreach (FileInfo f in dir.GetFiles("*", SearchOption.AllDirectories))
           {
               Servermap.Add(TurnPath(f.FullName),f.FullName);
           }
          
           //Client Part
           string clientPath = foldername.Substring(foldername.IndexOf("Uploads") + 7);
           string DownloadPath = System.IO.Path.GetFullPath("../../DownLoad");
           DirectoryInfo download = new DirectoryInfo(DownloadPath);
           if (!download.Exists) Directory.CreateDirectory(DownloadPath);
           clientPath = DownloadPath + clientPath;
           DirectoryInfo syncFolder = new DirectoryInfo(clientPath);
           if(syncFolder.Exists)
           foreach (FileInfo f in syncFolder.GetFiles("*", SearchOption.AllDirectories))
           {
               Clientset.Add(TurnPath(f.FullName));
           }
           
           string serverpart;
           foreach(string path in Clientset){
               
               string clientpart = DownloadPath + path;
               if(Servermap.TryGetValue(path,out serverpart))
               {
                   
                   CheckFileSame(serverpart, clientpart,ref a);
                   Servermap.Remove(path);
               }
               else
               {
                   d.Add(clientpart);
               }
           }
           foreach (KeyValuePair<string, string> file in Servermap)
           {
               a.Add(file.Value);
           }
       }

        async private Task SyncFolder()
        {
            List<string> AddItem = new List<string>();
            List<string> DeleteItem = new List<string>();
            CheckDiff(ref AddItem, ref DeleteItem);
            DeleteFiles(ref DeleteItem);
            
            foreach (string f in AddItem)
            {
                await Task.Run(() => tc.downLoadFile(f));
            }
            string messageBoxText = "Sync Completed!";
            string caption = "Sync Result";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
            RefreshTabs();
        }

        //Sync Folder
        private void SyncDir(object sender, RoutedEventArgs e)
        {
            Task task = SyncFolder();
        }

        ///Pop up download Menu
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


        //Pop up Sync Menu
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

