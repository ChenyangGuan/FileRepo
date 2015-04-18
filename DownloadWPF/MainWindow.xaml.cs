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


        private void Download(object sender, RoutedEventArgs e)
        {
            Task task = downloadFiles(Filename);
        }

        private void DownloadWithDepen(object sender, RoutedEventArgs e)
        {

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
            
        
    }

 }

