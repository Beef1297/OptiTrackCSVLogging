using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* for writing/reading csv */
using System.IO;
using System.Text;
using System.Linq;
using System;

/// <summary>
/// データをCSV に書き出す
/// </summary>
public class CSVLogging : MonoBehaviour
{
    [SerializeField] protected string filePath; // csv 保存するパス (relative? absolute?)
    [SerializeField] protected string fileName = "hoge.csv"; // csv ファイル名
    [SerializeField] protected GameObject[] targetPositionAndRotations; // 保存するデータたち

    // ステート管理のためのフラグたち
    // これ以外の方法があったはずだけど...
    private bool isStartMeasurement = false;
    private bool isWriteCSV = false;

    // 時間管理
    private float timeStamp = 0f;
    private float startTime = 0f;
    private float measurementTime = 10.0f;

    /// <summary>
    /// 計測中は，データをリストとして持っておいて最後にcsv に書き出す
    /// rotations として，オイラー角にするかQuaternion にするか悩んでる
    /// 座標変換とかを考えると Quaternion のほうが楽？
    /// </summary>
    public class Data
    {
        static public List<string> suffixes = new List<string>() { "-px", "-py", "-pz", "-qx", "-qy", "-qz", "-qz" };

        public List<float> px { get; set; }
        public List<float> py { get; set; }
        public List<float> pz { get; set; }
        public List<float> qx { get; set; }
        public List<float> qy { get; set; }
        public List<float> qz { get; set; }
        public List<float> qw { get; set; }

        public Data()
        {
            px = new List<float>();
            py = new List<float>();
            pz = new List<float>();
            qx = new List<float>();
            qy = new List<float>();
            qz = new List<float>();
            qw = new List<float>();
        }
    }

    private Dictionary<string, Data> datas;
    // data の数を数える
    private int dataNum = 0;

    private StreamWriter sw;

    void Start()
    {
        // ここらへん連続して実行するとデータが吹き飛ぶ可能性があるので修正が必要
        filePath = Application.dataPath + "/data/";
        if (fileName == "")
        {
            fileName = "hoge" + System.DateTime.Now.ToString("yyyy-mm-dd-HH-mm") + ".csv";
        }

        datas = new Dictionary<string, Data>();
        foreach(var tpar in targetPositionAndRotations)
        {
            datas.Add(tpar.name, new Data());
        }

        sw = new StreamWriter(filePath + fileName, false, Encoding.UTF8);

        // fixedUpdate の FPS を 100Hz に
        Time.fixedDeltaTime = 0.01f;
    }

    /// <summary>
    /// 特にFPS気にしない処理は Update へ
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("start measurement");
            isStartMeasurement = true;
            startTime = Time.unscaledTime;
            timeStamp = startTime;
        }
    }

    /// <summary>
    /// FPS を安定させるために FixedUpdate で記録していく
    /// </summary>
    void FixedUpdate()
    {
        if (isStartMeasurement)
        {
            foreach (var tpar in targetPositionAndRotations)
            {
                datas[tpar.name].px.Add(tpar.transform.position.x);
                datas[tpar.name].py.Add(tpar.transform.position.y);
                datas[tpar.name].pz.Add(tpar.transform.position.z);
                datas[tpar.name].qx.Add(tpar.transform.rotation.x);
                datas[tpar.name].qy.Add(tpar.transform.rotation.y);
                datas[tpar.name].qz.Add(tpar.transform.rotation.z);
                datas[tpar.name].qw.Add(tpar.transform.rotation.w);
                //Debug.Log(tpar.transform.rotation);
            }
            dataNum++;

            timeStamp += Time.fixedDeltaTime;
            if ((timeStamp - startTime) >= measurementTime)
            {
                Debug.Log("End measurement, then start write CSV");
                isStartMeasurement = false;
                isWriteCSV = true;
            }
        }

        if (isWriteCSV)
        {
            writeCSV();
        }
    }

    /// <summary>
    /// CSVに書き出すデータを整形する
    /// </summary>
    void formattingData(ref float[][] writeData, ref float[,] formattedData)
    {
        // メモリ管理気を付ける
        int index = 0;
        foreach (var paq in datas.Values)
        {
            writeData[index++] = paq.px.ToArray();
            writeData[index++] = paq.py.ToArray();
            writeData[index++] = paq.pz.ToArray();
            writeData[index++] = paq.qx.ToArray();
            writeData[index++] = paq.qy.ToArray();
            writeData[index++] = paq.qz.ToArray();
            writeData[index++] = paq.qw.ToArray();
        }
        
        // 転置する作業
        for (int i = 0; i < formattedData.GetLength(0); i++)
        {
            for (int j = 0; j < formattedData.GetLength(1); j++)
            {
                formattedData[i, j] = writeData[j][i];   
            }
        }
    }

    void writeCSV()
    {
        // CSV ファイルのヘッダー
        List<string> headers = new List<string>();
        // position x, y, z and quaternion x, y, z, w
        
        foreach (var key in datas.Keys)
        {
            foreach (var suffix in Data.suffixes)
            {
                headers.Add(key + suffix);
            }
        }

        // それぞれのデータを連結して転置するという処理をしたい
        // 列ごとに並んでる方が見やすいので...
        float[][] writeData = new float[headers.Count][];
        float[,] formattedData = new float[dataNum, headers.Count];
        formattingData(ref writeData, ref formattedData);

        sw.WriteLine(string.Join(",", headers));
        for (int i = 0; i < formattedData.GetLength(0); i++)
        {
            string line = "";
            for (int j = 0; j < formattedData.GetLength(1); j++)
            {
                line += formattedData[i, j] + ",";
            }
            sw.WriteLine(line);
        }

        sw.Flush();
        sw.Close();

        isWriteCSV = false;
        Debug.Log("End write CSV");
    }
}
