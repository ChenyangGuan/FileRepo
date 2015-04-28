using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DownloadWPF
{
    class Files
    {
        public string Name { get; set; }
        public string Length { get; set; }
        public string LastTime { get; set; }
        public string FullName { get; set; }

    }

    class Folders
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public Folders()
        {
            this.folder = new ObservableCollection<Folders>();
            this.file = new ObservableCollection<Files>();
        }
        public ObservableCollection<Folders> folder { get; set; }
        public ObservableCollection<Files> file { get; set; }

        public IList Children
        {
            get
            {
                return new CompositeCollection()
            {
                new CollectionContainer() { Collection = folder },
                new CollectionContainer() { Collection = file }
            };
            }
        }
    }
}
