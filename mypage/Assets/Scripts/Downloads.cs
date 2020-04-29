using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Net;
using UnityEngine.Networking;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class Downloads:MonoBehaviour
{
    public static Downloads instance;

    public int loadSpace = 10;
    public int m_loadNum;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
    }
    
    public IEnumerator get_web_file(string _load_url, string _storage_path, System.Action<long, long, int> _UIAction)
    {
        var headRequest = UnityWebRequest.Head(_load_url);
        headRequest.timeout = 10;
        yield return headRequest.SendWebRequest();
        if (headRequest.isNetworkError)
        {
            MyMain.instance.showDialogPanel();
        }
        else
        {
            string header = headRequest.GetResponseHeader("Content-Length");
            long totalLength = long.Parse(header);
            Debug.Log("totalLength: " + totalLength);
            FileStream fs = File.Open(_storage_path, FileMode.Append);
            long fileLength = fs.Length;
            Debug.Log("fileLength: " + fileLength);


            bool isOK = true;
            if (fileLength < totalLength)
            {
                fs.Seek(fileLength, SeekOrigin.Begin);

                long next_nM = fileLength + loadSpace * 1024 * 1024;
                HttpWebRequest request = WebRequest.Create(_load_url) as HttpWebRequest;
                request.Timeout = 10000;
                //断点续传核心，设置远程访问文件流的起始位置
                request.AddRange((int)fileLength);
                WebResponse response = null;
                try
                {
                    response = request.GetResponse();
                }
                catch(Exception e)
                {
                    MyMain.instance.showDialogPanel();
                    Debug.LogError("ResponceException: " + e);
                    isOK = false;
                }
                if(isOK)
                {
                    Stream stream = response.GetResponseStream();
                    byte[] buffer = new byte[1024];
                    int length = stream.Read(buffer, 0, buffer.Length);
                    while (length > 0)
                    {
                        //如果Unity客户端关闭，停止下载
                        if (fileLength > next_nM)
                        {
                            break;
                        }
                        fs.Write(buffer, 0, length);
                        fileLength += length;

                        //类似尾递归
                        length = stream.Read(buffer, 0, buffer.Length);

                        _UIAction?.Invoke(fileLength, totalLength, m_loadNum);
                    }
                    stream.Close();
                    stream.Dispose();
                    request.Abort();

                    m_loadNum++;
                }

            }
            else
            {
                fs.Close();
                fs.Dispose();

                DownloadDone();
            }
            fs.Close();
            fs.Dispose();
            Debug.Log("end fileLength: " + fileLength);

            if (fileLength < totalLength)
            {
                if(isOK)
                {
                    StartCoroutine(get_web_file(_load_url, _storage_path, _UIAction));
                }
            }
            else
            {
                DownloadDone();
            }
            
        }
    }

    void DownloadDone()
    {
        Debug.Log("DownLoad Done");
        MyMain.instance.m_topShowText.text = "下载已完成, 验证文件中。。。";
        MyMain.instance.DownloadDone();
    }

    public void JudgeFile()
    {
        using (FileStream fs = File.Open(MyMain.instance._storage_path, FileMode.OpenOrCreate))
        {

        }
    }
    public string getFileMd5(string _path)
    {
        try
        {
            FileStream fs = new FileStream(_path, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();
            fs.Dispose();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString().ToUpper();
        }
        catch (Exception ex)
        {
            MyMain.instance.m_textProgress.text = ex.Message;
            throw new Exception("md5file() fail, error: " + ex.Message);
        }
    }

    public void DeleteFile()
    {
        string _path = MyMain.instance._storage_path;
        if(File.Exists(_path))
        {
            File.Delete(_path);
        }
        else
        {
            MyMain.instance.m_topShowText.text = "文件不存在";
        }
    }



}
