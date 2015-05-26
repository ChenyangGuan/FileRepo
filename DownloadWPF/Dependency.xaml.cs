using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DownloadWPF
{
    /// <summary>
    /// Interaction logic for Dependency.xaml
    /// </summary>
    public partial class Dependency : Window
    {
        public Dependency()
        {
            InitializeComponent();
        }


        public Dependency(ref List<string> data)
        {
            InitializeComponent();
            Data_load(ref data);
        }

        public void Data_load(ref List<string> data)
        {
            List<FileInfo> fset = new List<FileInfo>();
            foreach (string path in data)
            {
                fset.Add(new FileInfo(path));
            }
            DependencyFile.ItemsSource = fset;
        }
    }
}
