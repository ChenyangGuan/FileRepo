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
           // string url = "http://localhost:55664/api/File";
            string url = "http://localhost/api/File";
            tc = new Client.TestClient(url);
            foldername = "";
            Filename = "";

            RefreshTabs();
        }
        private void RefreshTabs(){
            directoryTreeView.Items.Clear();
            Directory_Load();
            
            ClientfileInfo.AutoGeneratingColumn += fileInfoColumn_Load;
        }
        private void Directory_Load()
        {
            var directory = new ObservableCollection<DirRecord>();
            string path = tc.getServerFileFolder();
            Folders root = Newtonsoft.Json.JsonConvert.DeserializeObject<Folders>(path);
            
            //directory.Add(
            //    new DirRecord
            //    {
            //        Info=new DirectoryInfo(path)
            //    }
            //    );
            //directoryTreeView.ItemsSource = directory;
            directoryTreeView.Items.Add(root);
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

        private void CheckDependency(object sender, RoutedEventArgs e)
        {
            List<string> Directory=tc.GetDependencies(Filename);
            List<string> files = new List<string>();
            foreach (string dir in Directory)
            {
                DirectoryInfo d = new DirectoryInfo(dir);
                foreach (FileInfo f in d.GetFiles())
                {
                    files.Add(f.FullName);
                }
            }
            Dependency DepenWindow = new Dependency(ref files);
            DepenWindow.Show();
        }

        private void ViewClientFile(object sender, RoutedEventArgs e)
        {
            Details filedetail;
            FileInfo f = ClientfileInfo.SelectedItem as FileInfo;
            if (f.Name.IndexOf(".jpg") != -1 || f.Name.IndexOf(".png") != -1)
            {
                filedetail = new Details(f.FullName);
            }
            else
            {
                string text = System.IO.File.ReadAllText(@f.FullName);
                filedetail = new Details(ClientfileInfo.SelectedItem.ToString(), text);
            }
            filedetail.Show();
        }

        private void ViewServerFile(object sender, RoutedEventArgs e)
        {
            Details filedetail;
            if (Filename.IndexOf(".jpg") != -1 || Filename.IndexOf(".png") != -1)
            {
                filedetail = new Details(Filename);
            }
            else
            {
                string FileText = tc.getServerFileText(Filename);
                Files f = directoryTreeView.SelectedItem as Files;
                filedetail = new Details(f.Name, FileText);
            }
            filedetail.Show();
        }
        private async void Download(object sender, RoutedEventArgs e)
        {
            await downloadFileAsync(Filename);
            string messageBoxText = "Download Completed!";
            string caption = "Download Result";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
            RefreshTabs();
        }

        private async void DownloadWithDepen(object sender, RoutedEventArgs e)
        {
            await downloadDepenAsync(Filename);
            string messageBoxText = "Download Completed!";
            string caption = "Download Result";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
            RefreshTabs();
        }

        //Download Dependencies
        async private Task downloadDepenAsync(string filename)
        {
            List<string> fileset = tc.GetDependencies(filename);
            fileset.Add(filename);
            foreach (string f in fileset)
            {
                if(f.LastIndexOf("\\")!=f.Length-1)
                    await Task.Run(() => tc.downLoadFileAsync(f));
                else
                await Task.Run(() => tc.downLoadFolder(f));
            }
            
        }
        async private Task downloadFileAsync(string filename)
        {

            await Task.Run(() => tc.downLoadFileAsync(filename));            
            
            
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

        private void DeleteEmptyDir(){
            string clientPath = foldername.Substring(foldername.IndexOf("Uploads") + 7);
            string DownloadPath = System.IO.Path.GetFullPath("../../DownLoad");
            clientPath = DownloadPath + clientPath;
            DirectoryInfo syncFolder = new DirectoryInfo(clientPath);
            if (syncFolder.Exists)
                foreach (DirectoryInfo d in syncFolder.GetDirectories("*", SearchOption.AllDirectories))
                {
                    if (d.GetFiles("*", SearchOption.AllDirectories).Count()==0)
                    {
                        Directory.Delete(d.FullName);
                    }
                }
        }

        async private Task SyncFolderAsync()
        {
            List<string> AddItem = new List<string>();
            List<string> DeleteItem = new List<string>();
            CheckDiff(ref AddItem, ref DeleteItem);
            DeleteFiles(ref DeleteItem);
            DeleteEmptyDir();
            foreach (string f in AddItem)
            {
                await Task.Run(() => tc.downLoadFileAsync(f));
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
            Task task = SyncFolderAsync();
        }

        ///Pop up download Menu
        //private void ShowMenu(object sender, RoutedEventArgs e)
        //{
        //    object temp;
        //    temp = directoryTreeView.SelectedItem;
        //    if (temp == null) return;          
        //    var tmp = temp as FileInfo;
        //    Filename = tmp.FullName;
        //    System.Windows.Controls.ContextMenu menu = directoryTreeView.FindResource("downloadMenu") as System.Windows.Controls.ContextMenu;
        //    menu.PlacementTarget = sender as System.Windows.Controls.Button;
        //    menu.IsOpen = true;
        //}


        //Pop up Sync Menu
        private void ShowSyncMenu(object sender, RoutedEventArgs e)
        {
            object temp;
            temp = directoryTreeView.SelectedItem;
            if (temp == null) return;
           
                var tmp = temp as Folders;
                if (tmp != null)
                    foldername = tmp.FullName;
                else
                {
                    var tmp1 = temp as Files;
                    Filename = tmp1.FullName;
                }
            FrameworkElement element = sender as FrameworkElement;
            if (element.ContextMenu != null)
            {
                element.ContextMenu.PlacementTarget = element;
                element.ContextMenu.IsOpen = true;
            }
            //System.Windows.Controls.ContextMenu menu = directoryTreeView.FindResource("SyncMenu") as System.Windows.Controls.ContextMenu; 
            //menu.PlacementTarget = sender as System.Windows.Controls.Button;
            //menu.IsOpen = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshTabs();
        }
            
        
    }

 }

