using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
namespace FileRepository.Models
{
    class ServerFiles
    {
        public string Name { get; set; }
        public string Length { get; set; }
        public string LastTime { get; set; }
        public string FullName { get; set; }

    }

    class ServerFolders
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public ServerFolders()
        {
            this.folder = new ObservableCollection<ServerFolders>();
            this.file = new ObservableCollection<ServerFiles>();
        }
        public ObservableCollection<ServerFolders> folder { get; set; }
        public ObservableCollection<ServerFiles> file { get; set; }

        
    }
}