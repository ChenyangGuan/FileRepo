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
        
        IAsyncResult cbResult;
        Client.TestClient tc;
        string foldername;
        public MainWindow()
        {
            InitializeComponent();
            string url = "http://localhost:55664/FileService/api/File";
            tc = new Client.TestClient(url);
            foldername = "";
            tabs.Items.Remove(RenameTab);
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
                "Name", "Length", "FullName",  "LastWriteTime"
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

        void addFile(string file)
        {
            filepool.Items.Add(file);
        }
        void search(string path, string []pattern)
        {
            /* called on asynch delegate's thread */
            if (Dispatcher.CheckAccess())
                showPath(path);
            else
                Dispatcher.Invoke(
                  new Action<string>(showPath),
                  System.Windows.Threading.DispatcherPriority.Background,
                  new string[] { path }
                );
            string[] files;
            if (pattern.Length == 1 && pattern[0] == ".*") 
                files = System.IO.Directory.GetFiles(path, "*.*").ToArray();
             else files = System.IO.Directory.GetFiles(path, "*.*").Where(f => pattern.Contains(new FileInfo(f).Extension.ToLower())).ToArray();
            foreach (string file in files)
            {
                if (Dispatcher.CheckAccess())
                    addFile(file);
                else
                    Dispatcher.Invoke(
                      new Action<string>(addFile),
                      System.Windows.Threading.DispatcherPriority.Background,
                      new string[] { file }
                    );
            }
            string[] dirs = System.IO.Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                if (Dispatcher.CheckAccess())
                    addFile(dir);
                Dispatcher.Invoke(
                      new Action<string>(addFile),
                      System.Windows.Threading.DispatcherPriority.Background,
                      new string[] { dir }
                    );
            }
            if (Dispatcher.CheckAccess())
                showPath(path);
            else
                Dispatcher.Invoke(
                  new Action<string>(showPath),
                  System.Windows.Threading.DispatcherPriority.Background,
                  new string[] { path }
                );
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            filepool.Items.Clear();
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            string path = AppDomain.CurrentDomain.BaseDirectory;
            dlg.SelectedPath = path;
            DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                path = dlg.SelectedPath;
                hiddenpath.Text = path;
                string []pattern = {".*"};
                Action<string, string[]> proc = this.search;
                cbResult = proc.BeginInvoke(path, pattern, null, null);
                
            }
            
        }
      async private Task uploadFiles(string path){
            DirectoryInfo dir = new DirectoryInfo(path);
            
            foreach (FileInfo f in dir.GetFiles())
            {
                string filename = f.Name;
                if (foldername != "")
                {
                    filename = foldername + "\\" + filename;
                }
              await Task.Run(()=>tc.upLoadFile(filename, path));
            }
            string messageBoxText = "Upload Completed!";
            string caption = "Upload Result";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
            Directory_Load();
            fileInfo.AutoGeneratingColumn += fileInfoColumn_Load;
        }
        private void Upload_Async_Click(object sender, RoutedEventArgs e)
        {
            string path = hiddenpath.Text;
            
            Task task= uploadFiles(path);
            
        }

        private void Tab_DoubleClick(object sender, EventArgs e)
        {
           
            TabItem tab = sender as TabItem;
            this.tabs.Items.Remove(tab);
        }


       
        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            tabs.Items.Add(RenameTab);
            RenameTab.IsSelected = true;
        }

         private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            string[] AllNumIsSame = {"’", "”", "。", ";", ":", "<", ">", "?", "|", "!", "#", "$", "%", "^", "&", "*", "(", ")", "+", "-", "."};
            foreach(string chars in AllNumIsSame){
                if (NewName.Text.IndexOf(chars) != -1)
                {
                    Info.Content = "Can't contain the character " + chars;
                    return;
                }
            }
            foldername = NewName.Text;
            System.Windows.Controls.Button but = sender as System.Windows.Controls.Button;
            var parent = but.Parent as FrameworkElement;
            Info.Content = "";           
            var tab = parent.FindName("RenameTab");
            this.tabs.Items.Remove(tab);
            Renameinfo.Content = "Rename folder name completed.Use new folder name: "+foldername;
        }
        
        
    }
}
