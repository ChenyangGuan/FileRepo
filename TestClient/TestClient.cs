///////////////////////////////////////////////////////////////////////////
// TestClient.cs - Demonstrates how to test a file handling service      //
//                                                                       //
// Jim Fawcett, CSE686 - Internet Programming, Spring 2014               //
///////////////////////////////////////////////////////////////////////////
// started with C# Console Application                                   //
///////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;       // need to add reference to System.Web
using System.Net;       // need to add reference to System.Net
using System.Net.Http;  // need to add reference to System.Net.Http
using Newtonsoft.Json;  // need to add reference to System.Json
using System.Threading;

namespace Client
{
  public class TestClient
  {
    private HttpClient client = new HttpClient();
    private HttpRequestMessage message;
    private HttpResponseMessage response = new HttpResponseMessage();
    public string urlBase{get;set;}
    
    public string status { get; set; }

    //----< set destination url >------------------------------------------

    public TestClient(string url) { urlBase = url; }

    //----< get list of files available for download >---------------------

    //string[] getAvailableFiles()
    //{
    //  message = new HttpRequestMessage();
    //  message.Method = HttpMethod.Get;
    //  message.RequestUri = new Uri(urlBase);
    //  Task<HttpResponseMessage> task = client.SendAsync(message);
    //  HttpResponseMessage response1 = task.Result;
    //  response = task.Result;
    //  status = response.ReasonPhrase;
    //  string[] files = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(response1.Content.ReadAsStringAsync().Result);
    //  return files;
    //}


    public string getServerFileFolder()
    {
        message = new HttpRequestMessage();
        message.Method = HttpMethod.Get;
        message.RequestUri = new Uri(urlBase);
        Task<HttpResponseMessage> task = client.SendAsync(message);
        HttpResponseMessage response1 = task.Result;
        response = task.Result;
        status = response.ReasonPhrase;
        string path=Newtonsoft.Json.JsonConvert.DeserializeObject<string>(response1.Content.ReadAsStringAsync().Result);
        return path;
    }
    //----< open file on server for reading >------------------------------
    
    int openServerDownLoadFile(string fileName)
    {
      message = new HttpRequestMessage();
      message.Method = HttpMethod.Get;
      string urlActn = "?fileName=" + fileName + "&open=download";
      message.RequestUri = new Uri(urlBase + urlActn);
      Task<HttpResponseMessage> task = client.SendAsync(message);
      HttpResponseMessage response = task.Result;
      status = response.ReasonPhrase;
      return (int)response.StatusCode;
    }
    //----< open file on client for writing >------------------------------

    FileStream openClientDownLoadFile(string fileName)
    {
      string path = "../../DownLoad/";
      string abpath = Path.GetFullPath(path);
      string filedir="";
        if(fileName.IndexOf("Uploads")!=-1)
      filedir= fileName.Substring(fileName.IndexOf("Uploads") + 8,fileName.LastIndexOf("\\")-fileName.IndexOf("Uploads")-8);
        else if(fileName.IndexOf("App_Data")!=-1)
      filedir = fileName.Substring(fileName.IndexOf("App_Data") + 9, fileName.LastIndexOf("\\") - fileName.IndexOf("App_Data") - 9);
      abpath += filedir+"\\";
      DirectoryInfo dir = new DirectoryInfo(abpath);
      if (!dir.Exists) Directory.CreateDirectory(abpath);
      string partname = fileName.Substring(fileName.LastIndexOf("\\") + 1);
      FileStream down;
      try
      {
          down = new FileStream(abpath + partname, FileMode.OpenOrCreate);
      }
      catch
      {
        return null;
      }
      return down;
    }
    //----< read block from server file and write to client file >---------

    byte[] getFileBlock(FileStream down, int blockSize)
    {
      message = new HttpRequestMessage();
      message.Method = HttpMethod.Get;
      string urlActn = "?blockSize=" + blockSize.ToString();
      message.RequestUri = new Uri(urlBase + urlActn);
      Task<HttpResponseMessage> task = client.SendAsync(message);
      HttpResponseMessage response = task.Result;
      Task<byte[]> taskb = response.Content.ReadAsByteArrayAsync();
      byte[] Block =taskb.Result;
      status = response.ReasonPhrase;
      return Block;
    }
    //----< close FileStream on server and FileStream on client >----------

    void closeServerFile()
    {
      message = new HttpRequestMessage();
      message.Method = HttpMethod.Get;
      string urlActn = "?fileName=dontCare.txt&open=close";
      message.RequestUri = new Uri(urlBase + urlActn);
      Task<HttpResponseMessage> task = client.SendAsync(message);
      HttpResponseMessage response = task.Result;
      status = response.ReasonPhrase;
    }
    //----< open file on server for writing >------------------------------

    int openServerUpLoadFile(string fileName)
    {
      
      message = new HttpRequestMessage();
      message.Method = HttpMethod.Get;
      string urlActn = "?fileName=" + fileName + "&open=upload";
      message.RequestUri = new Uri(urlBase + urlActn);
      Task<HttpResponseMessage> task = client.SendAsync(message);
      HttpResponseMessage response = task.Result;
      status = response.ReasonPhrase;
      return (int)response.StatusCode;
    }
    //----< open file on client for Reading >------------------------------

    FileStream openClientUpLoadFile(string fileName)
    {
    
      FileStream up;
      try
      {
        up = new FileStream(fileName, FileMode.Open);
      }
      catch
      {
        return null;
      }
      return up;
    }
    //----< post blocks to server >----------------------------------------
    void putBlock(byte[] Block)
    {
      message = new HttpRequestMessage();
      message.Method = HttpMethod.Post;
      message.Content = new ByteArrayContent(Block);
      message.Content.Headers.Add("Content-Type","application/http;msgtype=request");
      string urlActn = "?blockSize=" + Block.Count().ToString();
      message.RequestUri = new Uri(urlBase + urlActn);
      Task<HttpResponseMessage> task = client.SendAsync(message);
      HttpResponseMessage response = task.Result;
      status = response.ReasonPhrase;
    }

    //----< downLoad Folder>-----------------------------------------------
    async public Task downLoadFolder(string foldername)
    {
        DirectoryInfo dir = new DirectoryInfo(foldername);
        foreach (FileInfo f in dir.GetFiles())
        {
            await Task.Run(() => this.downLoadFileAsync(f.FullName));
        }
            
    }

    //----< downLoad File >------------------------------------------------
    /*
     *  Open server file for reading
     *  Open client file for writing
     *  Get blocks from server
     *  Write blocks to local file
     *  Close server file
     *  Close client file
     */
    public async void downLoadFileAsync(string filename)
    {
      Console.Write("\n  Attempting to download file {0} ", filename);
      Console.Write("\n ------------------------------------------\n");
      FileStream down;
      Console.Write("\n  Sending Get request to open file");
      Console.Write("\n ----------------------------------");
      int status = openServerDownLoadFile(filename);
      Console.Write("\n  Response status = {0}\n", status);
      if (status >= 400)
        return;
      down = openClientDownLoadFile(filename);

      Console.Write("\n  Sending Get requests for block from file");
      Console.Write("\n ------------------------------------------");
      while (true)
      {
        int blockSize = 204800;
        byte[] Block = getFileBlock(down, blockSize);
        
        Console.Write("\n  Response status = {0}", status);
        Console.Write("\n  received block of size {0} bytes\n", Block.Length);
        if (Block.Length == 0 || blockSize <= 0)
          break;
        down.Write(Block, 0, Block.Length);
        if (Block.Length < blockSize)    // last block
          break;
      }

      Console.Write("\n  Sending Get request to close file");
      Console.Write("\n -----------------------------------");

      closeServerFile();
      Console.Write("\n  Response status = {0}\n", status);
      down.Close();
    }

    //----< Get Dependencies File >---------------------------------------
public List<string> GetDependencies(string filename){  
        List<string> fileset=new List<string>();
        message = new HttpRequestMessage();
        message.Method = HttpMethod.Get;
        message.RequestUri = new Uri(urlBase+"?Fullpath="+filename);
        Task<HttpResponseMessage> task = client.SendAsync(message);
        HttpResponseMessage response1 = task.Result;
        response = task.Result;
        status = response.ReasonPhrase;
        fileset = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(response1.Content.ReadAsStringAsync().Result);        
        foreach (string f in fileset)
        {
            Console.Write("\n <file> {0}",  f);
        }
        return fileset;
    
}
     //----<upload Folder >-----------------------------------------------
async public Task upLoadFolder(string foldername, string newname, string path)
{
    DirectoryInfo dir = new DirectoryInfo(foldername);
    string filepath=path;
    if (newname != "") filepath = newname+"\\";
    foreach(FileInfo f in dir.GetFiles())
    {
        await upLoadFile(f.FullName, filepath);
    }
    foreach (DirectoryInfo f in dir.GetDirectories())
    {
       await upLoadFolder(f.FullName, "", path+f.Name+"\\");
    }
}
    //----< upLoad File >--------------------------------------------------
    /*
     *  Open server file for writing
     *  Open client file for reading
     *  Read blocks from local file
     *  Send blocks to server
     *  Close server file
     *  Close client file
     */
     async public Task upLoadFile(string filename,string path)
    {
      Console.Write("\n  Attempting to upload file {0}", filename);
      Console.Write("\n --------------------------------------\n");

      Console.Write("\n  Sending get request to open file");
      Console.Write("\n ----------------------------------");
      string serveruploadfolder = getServerFileFolder();
      string FilenameOnServer = Path.GetFullPath(serveruploadfolder)+path+filename.Substring(filename.LastIndexOf("\\")+1);
      openServerUpLoadFile(FilenameOnServer);
      Console.Write("\n  Response status = {0}\n", status);
      
      FileStream up = openClientUpLoadFile(filename);

      Console.Write("\n  Sending Post requests to send blocks:");
      Console.Write("\n ---------------------------------------");

      const int upBlockSize = 204800;
      byte[] upBlock = new byte[upBlockSize];
      int bytesRead = upBlockSize;
      while (bytesRead == upBlockSize)
      {
        bytesRead = up.Read(upBlock, 0, upBlockSize);
        if (bytesRead < upBlockSize)
        {
          byte[] temp = new byte[bytesRead];
          for (int i = 0; i < bytesRead; ++i)
            temp[i] = upBlock[i];
          upBlock = temp;
        }
        Console.Write("\n  sending block of size {0}", upBlock.Count());
        putBlock(upBlock);
        Console.Write("\n  status = {0}\n", status);
      }

      Console.Write("\n  Sending Get request to close file");
      Console.Write("\n -----------------------------------");

      closeServerFile();
      Console.Write("\n  Response status = {0}\n", status);
      up.Close();
    }

    //static List<string> GetUploadFiles(string path)
    //{
    //    DirectoryInfo dir;
    //    if(path=="")
    //     dir = new DirectoryInfo(@"../../uploads");
    //    else  dir = new DirectoryInfo(@path);
    //    int index = 1;
    //    List<string> fileset = new List<string>();
    //    foreach (FileInfo f in dir.GetFiles())
    //    {
    //        string fullname = f.FullName;
    //        string fn = fullname.Substring(fullname.LastIndexOf("\\") + 1);
    //        fileset.Add(fn);
    //        Console.Write("\n {0}. {1}", index, fn);
    //        index++;
    //    }
    //    return fileset;
    //}
    
    //static void upload(TestClient tc)
    //{

    //    Console.Write("\n  Choose available file to upload:");
    //    Console.Write("\n ------------------------------------------");
    //    string path = "";
    //    Console.Write("\n Enter the file path: ");
    //    path=Console.ReadLine();
    //    List<string> fileset = GetUploadFiles(path);
    //    int choice = 0;
    //    while (true)
    //    {
    //        Console.Write("\n Please choose a file to upload:  ");
    //        try
    //        {
    //             choice = int.Parse(Console.ReadLine());
    //        }
    //        catch (System.FormatException)
    //        {
    //            Console.Write("\n Error enter, try again.  ");
    //            continue;
    //        }
    //        if (choice > fileset.Count)
    //        {
    //            Console.Write("\n Error enter, try again.  ");
    //            continue;
    //        }
    //        else break;
    //    }
        
    //    string uploadFile = fileset[choice - 1];
    //    tc.upLoadFile(uploadFile,path);
    //    Console.Write("\n Successful !!!!  ");
    //    Console.ReadLine();
    //}
    // public string[] GetdownloadFiles(string path,int index)
    //{
    //    string[] fileset;
    //    message = new HttpRequestMessage();
    //    message.Method = HttpMethod.Get;
    //    message.RequestUri = new Uri(urlBase+"?fullpath="+path+"&&ford=true");
    //    Task<HttpResponseMessage> task = client.SendAsync(message);
    //    HttpResponseMessage response1 = task.Result;
    //    response = task.Result;
    //    status = response.ReasonPhrase;
    //    fileset = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(response1.Content.ReadAsStringAsync().Result);        
    //    foreach (string f in fileset)
    //    {
    //        Console.Write("\n <file> {0}. {1}", index, f);
    //    }
    //    return fileset;
    //}


     //public string[] GetdownloadDir(string path)
     //{
     //    string[] dirset;
     //    message = new HttpRequestMessage();
     //    message.Method = HttpMethod.Get;
     //    message.RequestUri = new Uri(urlBase+"?fullpath="+path+"&&ford=false");
     //    Task<HttpResponseMessage> task = client.SendAsync(message);
     //    HttpResponseMessage response1 = task.Result;
     //    response = task.Result;
     //    status = response.ReasonPhrase;
     //    int index = 1;
     //    dirset = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(response1.Content.ReadAsStringAsync().Result);
     //    foreach (string f in dirset)
     //    {
     //        Console.Write("\n <dir> {0}. {1}", index++, f);
     //    }
     //    return dirset;
     //}

     

    //static void download(TestClient tc)
    //{

    //    Console.Write("\n  Choose available file to download:");
    //    Console.Write("\n ------------------------------------------");
    //    string dirpath = "../../uploads/";
        
    //    string downloadf = "";
    //    while (true)
    //    {
            
    //        string[] dirset = tc.GetdownloadDir(dirpath);
    //        int index = dirset.Length;
    //        string[] fileset = tc.GetdownloadFiles(dirpath, ++index);
    //        int choice = 0;
    //        int top = index+fileset.Length;
    //        Console.Write("\n Please choose a file to download or open a directory:  ");
    //        try
    //        {
    //             choice = int.Parse(Console.ReadLine());
    //        }
    //        catch (System.FormatException)
    //        {
    //            continue;
    //        }

    //        if (choice < index)
    //        {
    //            dirpath += dirset[choice - 1] + "/";
    //            continue;
    //        }
    //        else if (choice < top)
    //        {
    //            downloadf = fileset[choice - 1];
    //            break;
    //        }
    //        else continue;
    //    }
    //    tc.downLoadFile(downloadf);
    //    Console.Write("\n Successful !!!!  ");
    //    Console.ReadLine();
    //}
    
    static void Main(string[] args)
    {
      
      TestClient tc = new TestClient("http://localhost:55664/FileService/api/File");    
  //      Console.Write("\n  Waiting for server to initialize\n");
  //      Thread.Sleep(100);
  //      int num = 0;
  //          while (true)
  //          {
  //              Console.Write("\n  Demonstrating WebApi File Service and Test Client");
  //              Console.Write("\n ===================================================\n");
  //              Console.Write("\n *****1. Upload File*****\n");
  //              Console.Write("\n *****2. Download File*****\n");            
  //              Console.Write("\n *****3. Exit*****\n");
  //              Console.Write("\n What you want:  ");
  //              try
  //              {
  //                  num = int.Parse(Console.ReadLine());
  //              }
  //              catch (System.FormatException)
  //              {
  //                  Console.Clear();
  //                  continue;
  //              }
  //              switch (num)
  //              {
  //                  case 1: upload(tc);break;
  //                  case 2: download(tc); break;                    
  //                  case 3: Environment.Exit(0); break;
  //                  default: Console.Clear(); continue;

  //              }
  //              Console.Clear();
  //          }     
    }
  }
}
