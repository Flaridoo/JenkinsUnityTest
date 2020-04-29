using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyMain : MonoBehaviour
{
    public Text m_topShowText;
    public GameObject m_dialogPanel;
    public GameObject m_dialogBtn;
    public Text m_dialogText;
    public InputField m_inputNum;

    public GameObject m_btnSure;
    public GameObject m_btnDelete;
    public Text m_textProgress;

    public string _load_url = "http://xjzn2.oss-cn-hangzhou.aliyuncs.com/test_zph/16/16.2.zip";
    public string _storage_path = "D:/16.3.zip";

    public static MyMain instance;

    private void Awake()
    {
        instance = this;
        _load_url = "http://xjzn2.oss-cn-hangzhou.aliyuncs.com/test_zph/16/16.2.zip";
    }

    // Start is called before the first frame update
    void Start()
    {
        m_btnSure.SetActive(true);
        m_textProgress.gameObject.SetActive(false);
        m_dialogBtn.GetComponent<Button>().onClick.AddListener(StartDownloads);
        m_btnSure.GetComponent<Button>().onClick.AddListener(StartDownloads);
        m_btnDelete.GetComponent<Button>().onClick.AddListener(Downloads.instance.DeleteFile);

        _storage_path = Application.isEditor ? _storage_path : Application.persistentDataPath + "/16.3.zip";
        m_topShowText.text = "Are you ready?";
    }

    void StartDownloads()
    {
        Downloads.instance.JudgeFile();
        try
        {
            Downloads.instance.loadSpace = int.Parse(m_inputNum.text);
        }
        catch
        {
            m_topShowText.text = "你输入的是：" + m_inputNum.text + "\n请输入正确的数字";
            return;
        }

        m_inputNum.gameObject.SetActive(false);
        m_btnSure.SetActive(false);
        m_textProgress.gameObject.SetActive(true);
        m_dialogPanel.SetActive(false);
        m_btnDelete.SetActive(false);

        m_topShowText.text = "准备中。。。";
        m_textProgress.text = "准备中。。。";

        StartCoroutine(Downloads.instance.get_web_file(_load_url, _storage_path,
            (long fileLength, long totalLength, int loadNum) =>
            {
                string percent = string.Format("{0:F2}", ((float)fileLength / totalLength) * 100);
                m_topShowText.text = "已下载次数：" + loadNum + ", 下载中。。。";
                m_textProgress.text = percent + "%";
            })
        );
    }
    
    public void showDialogPanel()
    {
        m_dialogPanel.SetActive(true);
        m_dialogText.text = "网络异常，请稍后重试";
    }

    public void DownloadDone()
    {
        string md5 = Downloads.instance.getFileMd5(_storage_path);
        m_textProgress.text = "File MD5: " + md5;
        m_topShowText.text = "下载次数：" + Downloads.instance.m_loadNum +
        //"\n\n文件是否符合：" + (md5 == "00351D9A5C31333AC2EFF7F9C7639855");
        "\n\n文件是否符合：" + (md5 == "8A9A0EA4271A758140348170A21E00F4");
    }
}
