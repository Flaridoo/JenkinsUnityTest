using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;

public class HttpHelper : MonoBehaviour
{
    private string downloadUrl = "";//文件下载地址
    private string filePath = "";//文件保存路径
    private string method = "";//方法
    private long fromIndex = 0;//开始下载的位置
    private long toIndex = 0;//结束下载的位置
    private long count = 0;//总大小
    private long size = 1024 * 50;//每次下载大小 512kb

    public static HttpHelper instance;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void StartDownload()
    {
        WebResponse rsp;
        while (this.fromIndex < this.toIndex)
        {
            long to;
            if (this.fromIndex + this.size >= this.toIndex - 1)
                to = this.toIndex - 1;
            else
                to = this.fromIndex + size;
            using (rsp = Download(this.downloadUrl, this.fromIndex, to, this.method))
            {
                Save(this.filePath, rsp.GetResponseStream());
            }
        }
        if (this.fromIndex >= this.toIndex)
        {
            //this.isFinish = true;
            //this.isStopped = true;
            //OnFineshHandler();
        }
    }


    private void Save(string filePath, Stream stream)
    {
        try
        {
            using (var writer = File.Open(filePath, FileMode.Append))
            {
                using (stream)
                {
                    var repeatTimes = 0;
                    byte[] buffer = new byte[1024];
                    var length = 0;
                    while ((length = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        writer.Write(buffer, 0, length);
                        this.fromIndex += length;
                        if (repeatTimes % 5 == 0)
                        {
                            //OnDownloadHandler();
                        }
                        repeatTimes++;
                    }
                }
            }
            //OnDownloadHandler();
        }
        catch (Exception)
        {
            //异常也不影响
        }
    }



    public WebResponse Download(string downloadUrl, long from, long to, string method)
    {
        var request = GetHttpWebRequest(downloadUrl);
        init_Request(ref request);
        request.Accept = "text/json,*/*;q=0.5";
        request.AddRange(from, to);
        request.Headers.Add("Accept-Charset", "utf-8;q=0.7,*;q=0.7");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, x-gzip, identity; q=0.9");
        request.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
        request.Timeout = 120000;
        request.Method = method;
        request.KeepAlive = false;
        request.ContentType = "application/json; charset=utf-8";
        return request.GetResponse();
    }
    public System.Net.HttpWebRequest GetHttpWebRequest(string url)
    {
        HttpWebRequest request = null;
        if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            request = WebRequest.Create(url) as HttpWebRequest;
            request.ProtocolVersion = HttpVersion.Version10;
        }
        else
        {
            request = WebRequest.Create(url) as HttpWebRequest;
        }
        return request;
    }
    public void init_Request(ref System.Net.HttpWebRequest request)
    {
        request.Accept = "text/json,*/*;q=0.5";
        request.Headers.Add("Accept-Charset", "utf-8;q=0.7,*;q=0.7");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, x-gzip, identity; q=0.9");
        request.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
        request.Timeout = 8000;
    }
    private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
    {
        return true; //总是接受  
    }
}












    

/// <summary>
/// 通过http下载资源
/// </summary>
public class HttpDownLoads
{
    //下载进度
    public float progress { get; private set; }
    //涉及子线程要注意,Unity关闭的时候子线程不会关闭，所以要有一个标识
    private bool isStop;
    //子线程负责下载，否则会阻塞主线程，Unity界面会卡主
    private Thread thread;
    //表示下载是否完成
    public bool isDone { get; private set; }


    /// <summary>
    /// 下载方法(断点续传)
    /// </summary>
    /// <param name="url">URL下载地址</param>
    /// <param name="savePath">Save path保存路径</param>
    /// <param name="callBack">Call back回调函数</param>
    public void DownLoad(string url, string savePath, Action callBack)
    {
        isStop = false;
        //开启子线程下载,使用匿名方法
        thread = new Thread(delegate () {
            //判断保存路径是否存在
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            //这是要下载的文件名，比如从服务器下载a.zip到D盘，保存的文件名是test
            string filePath = savePath + "/test";

            //使用流操作文件
            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            //获取文件现在的长度
            long fileLength = fs.Length;
            //获取下载文件的总长度
            long totalLength = GetLength(url);

            //如果没下载完
            if (fileLength < totalLength)
            {
                //断点续传核心，设置本地文件流的起始位置
                fs.Seek(fileLength, SeekOrigin.Begin);

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

                //断点续传核心，设置远程访问文件流的起始位置
                request.AddRange((int)fileLength);
                Stream stream = request.GetResponse().GetResponseStream();

                byte[] buffer = new byte[1024];
                //使用流读取内容到buffer中
                //注意方法返回值代表读取的实际长度,并不是buffer有多大，stream就会读进去多少
                int length = stream.Read(buffer, 0, buffer.Length);
                while (length > 0)
                {
                    //如果Unity客户端关闭，停止下载
                    if (isStop) break;
                    fs.Write(buffer, 0, length);
                    fileLength += length;

                    //类似尾递归
                    length = stream.Read(buffer, 0, buffer.Length);
                }
                stream.Close();
                stream.Dispose();

            }
            else
            {
                progress = 1;
            }
            fs.Close();
            fs.Dispose();
            //如果下载完毕，执行回调
            if (progress == 1)
            {
                isDone = true;
                if (callBack != null) callBack();
            }

        });
        //开启子线程
        thread.IsBackground = true;
        thread.Start();
    }

    /// <summary>
    /// 获取下载文件的大小
    /// </summary>
    /// <returns>The length.</returns>
    /// <param name="url">URL.</param>
    long GetLength(string url)
    {
        HttpWebRequest requet = HttpWebRequest.Create(url) as HttpWebRequest;
        requet.Method = "HEAD";
        HttpWebResponse response = requet.GetResponse() as HttpWebResponse;
        return response.ContentLength;
    }

    public void Close()
    {
        isStop = true;
    }
    
}
