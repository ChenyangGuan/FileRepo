
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace UploadWPF
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        
        Client.TestClient tc;
        string Foldername;
        string NewFoldername;
        string SelectFolder;
        string Filename;
        public MainWindow()
        {
            InitializeComponent();
           // string url = "http://localhost:55664/api/File";
            string url = "http://localhost/api/File";
            tc = new Client.TestClient(url);
            Foldername = "";
            SelectFolder = "";
            NewFoldername = "";
            tabs.Items.Remove(RenameTab);
            Refresh();
        }

        private void Directory_Load()
        {
            var directory = new ObservableCollection<DirRecord>();
            string path = tc.getServerFileFolder();
            Folders root = Newtonsoft.Json.JsonConvert.DeserializeObject<Folders>(path);
            
            directoryTreeView.Items.Add(root);
        }

        private void SelectedFolder_Load(string path)
        {
            var directory = new ObservableCollection<DirRecord>();

            directory.Add(
                new DirRecord
                {
                    Info = new DirectoryInfo(path)
                }
                );
            SelectedFolderTreeView.ItemsSource = directory;
            
        }

        private void fileInfoColumn_Load(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            List<string> requiredProperties = new List<string>
            {
                "Name", "Length", "Extension",  "LastWriteTime"
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


        void showPath(string path)
        {
            pathpool.Content = path;
        }
        /*-- invoke on UI thread --------------------------------*/


        private void Refresh()
        {
            Directory_Load();
            
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            string path = AppDomain.CurrentDomain.BaseDirectory;
            dlg.SelectedPath = path;
            DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                path = dlg.SelectedPath;
                showPath(path);
                SelectFolder = path;
                hiddenpath.Text = path;
                SelectedFolder_Load(path) ;
                SelectedFolderFileInfo.AutoGeneratingColumn += fileInfoColumn_Load;
            }
            
        }
      async private Task Upload_Folder(){
          await Task.Run(() => tc.upLoadFolder(Foldername, NewFoldername, SelectFolder.Substring(SelectFolder.LastIndexOf("\\")+1)+"\\"));
            string messageBoxText = "Upload Completed!";
            string caption = "Upload Result";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
            Directory_Load();
           
        }
      async private Task Upload_file()
      {
          await Task.Run(() => tc.upLoadFile(Filename, SelectFolder.Substring(SelectFolder.LastIndexOf("\\")+1)+"\\"));
          string messageBoxText = "Upload Completed!";
          string caption = "Upload Result";
          MessageBoxButton button = MessageBoxButton.OK;
          MessageBoxImage icon = MessageBoxImage.Information;
          System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
          Refresh();
      }

        private void upload(object sender, RoutedEventArgs e)
        {
            Task task = Upload_file();
        }

        private void upload_folder(object sender, RoutedEventArgs e)
        {
            tabs.Items.Add(RenameTab);
            RenameTab.IsSelected = true;
            ChooseFolderTab.IsEnabled = false;
            
        }
        private void Tab_DoubleClick(object sender, EventArgs e)
        {
           
            TabItem tab = sender as TabItem;
            this.tabs.Items.Remove(tab);
        }


       
       

        private void ShowMenu(object sender, RoutedEventArgs e)
        {
            object temp;
            temp = SelectedFolderFileInfo.SelectedItem;
            if (temp == null) return;
            var tmp = temp as FileInfo;
            Filename = tmp.FullName;
            System.Windows.Controls.ContextMenu menu = SelectedFolderFileInfo.FindResource("uploadMenu") as System.Windows.Controls.ContextMenu;
            menu.PlacementTarget = sender as System.Windows.Controls.Button;
            menu.IsOpen = true;
        }

        private void ShowTreeMenu(object sender, RoutedEventArgs e)
        {
            object temp;
            temp = SelectedFolderTreeView.SelectedItem;
            if (temp == null) return;
            var tmp = temp as DirRecord;
            Foldername = tmp.Info.FullName;
            System.Windows.Controls.ContextMenu menu = SelectedFolderTreeView.FindResource("uploadFolderMenu") as System.Windows.Controls.ContextMenu;
            menu.PlacementTarget = sender as System.Windows.Controls.Button;
            menu.IsOpen = true;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            NewFoldername = NewName.Text.Trim();
            this.tabs.Items.Remove(RenameTab);
            ChooseFolderTab.IsEnabled = true;
            Task task = Upload_Folder();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.tabs.Items.Remove(RenameTab);
            ChooseFolderTab.IsEnabled = true;
            Task task = Upload_Folder();
        }
        // private void Confirm_Click(object sender, RoutedEventArgs e)
        //{
        //    if (NewName.Text.Trim() == "")
        //    {
        //        Renameinfo.Content = "                       Use default folder name, same as upload folder.";
        //        return;
        //    }
        //    string[] AllNumIsSame = {"’", "”", "。", ";", ":", "<", ">", "?", "|", "!", "#", "$", "%", "^", "&", "*", "(", ")", "+", "-", "."};
        //    foreach(string chars in AllNumIsSame){
        //        if (NewName.Text.IndexOf(chars) != -1)
        //        {
        //            Info.Content = "Can't contain the character " + chars;
        //            return;
        //        }
        //    }
        //    foldername = NewName.Text;
        //    System.Windows.Controls.Button but = sender as System.Windows.Controls.Button;
        //    var parent = but.Parent as FrameworkElement;
        //    Info.Content = "";           
        //    var tab = parent.FindName("RenameTab");
        //    this.tabs.Items.Remove(tab);
        //    Renameinfo.Content = "Rename folder name completed.Use new folder name: "+foldername;
        //}

        

       
        
        
    }
}
