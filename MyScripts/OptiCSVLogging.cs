using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* for writing/reading csv */
using System.IO;
using System.Text;

/// <summary>
/// データをCSV に書き出す
/// </summary>
public class OptiCSVLogging : CSVLogging {
    [Header("GameObjects attached OptiTrack Client Rigid Body")]
    [SerializeField] protected GameObject[] targetObjects; // 保存するデータたち
    [SerializeField] protected float FPS;

    private List<float> timeStamps;

    // ステート管理のためのフラグたち
    // これ以外の方法があったはずだけど...
    private bool isMeasuring = false;


    private Dictionary<string, Data> datas;
    private List<string> csvHeaders;
    // data の数を数える
    private int dataNum = 0;

    void initializeDataDictionary() {
        foreach (var tpar in targetObjects) {
            datas.Add(tpar.name, new Data());
        }
    }

    void updateDataDictionary() {
        foreach (var tpar in targetObjects) {
            datas[tpar.name].px.Add(tpar.transform.position.x);
            datas[tpar.name].py.Add(tpar.transform.position.y);
            datas[tpar.name].pz.Add(tpar.transform.position.z);
            datas[tpar.name].qx.Add(tpar.transform.rotation.x);
            datas[tpar.name].qy.Add(tpar.transform.rotation.y);
            datas[tpar.name].qz.Add(tpar.transform.rotation.z);
            datas[tpar.name].qw.Add(tpar.transform.rotation.w);
        }
    }

    override protected void Start() {
        base.Start();


        if (targetObjects.Length <= 0) {
            Debug.LogError("Objects for csv log were not set");
            Application.Quit();
        }
        datas = new Dictionary<string, Data>();
        initializeDataDictionary();
        timeStamps = new List<float>();
        // fixedUpdate の FPS を 100Hz に
        Time.fixedDeltaTime = 1.0f / FPS;
        // CSV ファイルのヘッダー
        csvHeaders = new List<string>();

        foreach (var key in datas.Keys) {
            foreach (var suffix in Data.suffixes) {
                csvHeaders.Add(key + suffix);
            }
        }
    }

    /// <summary>
    /// FPS を安定させるために FixedUpdate で記録していく
    /// </summary>
    void FixedUpdate() {
        if (isMeasuring) {
            timeStamps.Add(Time.unscaledTime);
            updateDataDictionary();
            dataNum++;

        }

    }

    override public void startLogging(string subjectName, string _filePrefix) {
        filePrefix = _filePrefix;
        fileName = subjectName + System.DateTime.Now.ToString("yyMMdd-HHmmss");
        isMeasuring = true;
    }

    override public void endLogging() {
        isMeasuring = false;
        writeCSV();
    }


    override protected void writeCSV() {


        // それぞれのデータを連結して転置するという処理をしたい
        // 列ごとに並んでる方が見やすいので...
        float[][] writeData = new float[csvHeaders.Count][];
        float[,] formattedData = new float[dataNum, csvHeaders.Count];
        Data.formattingData(ref datas, ref writeData, ref formattedData);

        sw = new StreamWriter(filePath + filePrefix + "-" + fileName + ".csv", false, Encoding.UTF8);
        sw.WriteLine("timestamp, " + string.Join(",", csvHeaders));  // header の書き出し
        for (int i = 0; i < formattedData.GetLength(0); i++) {
            string line = timeStamps[i].ToString() + ",";
            for (int j = 0; j < formattedData.GetLength(1); j++) {
                //TODO: data の種類に合わせてデータの指定子(?)を変えられるように
                line += string.Format("{0:f10},", formattedData[i, j]);
            }
            sw.WriteLine(line);
        }

        sw.Flush();
        sw.Close();

        // データの初期化
        // ガベージコレクション頼んだ～
        datas.Clear();
        initializeDataDictionary();
        timeStamps.Clear();
        dataNum = 0;
        sw = null;
        Debug.Log("End write CSV");
    }

    void OnDestroy() {
        if (sw != null) {
            sw.Flush();
            sw.Close();
        }
    }
}
