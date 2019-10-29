using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* for writing/reading csv */
using System.IO;
using System.Text;

/// <summary>
/// データをCSV に書き出す
/// </summary>
public class OptiCSVLogging : MonoBehaviour {
    [HeaderAttribute("File Path for CSV file")]
    [SerializeField] protected string filePath; // csv 保存するパス (relative? absolute?)
    [SerializeField] protected string subjectName; // 被験者名
    protected string fileName;
    [HeaderAttribute("GameObjects attached OptiTrack Client Rigid Body")]
    [SerializeField] protected GameObject[] targetPositionAndRotations; // 保存するデータたち

    private List<float> timeStamps;

    // ステート管理のためのフラグたち
    // これ以外の方法があったはずだけど...
    private bool isMeasuring = false;

    /// <summary>
    /// interface を作って汎用的にする
    /// </summary>
    public class Data {
        static public List<string> suffixes = new List<string>() { "-px", "-py", "-pz", "-qx", "-qy", "-qz", "-qz" };

        public List<float> px { get; set; }
        public List<float> py { get; set; }
        public List<float> pz { get; set; }
        public List<float> qx { get; set; }
        public List<float> qy { get; set; }
        public List<float> qz { get; set; }
        public List<float> qw { get; set; }

        public Data() {
            px = new List<float>();
            py = new List<float>();
            pz = new List<float>();
            qx = new List<float>();
            qy = new List<float>();
            qz = new List<float>();
            qw = new List<float>();
        }

        /// <summary>
        /// CSVに書き出すデータを整形する
        /// </summary>
        static public void formattingData(ref Dictionary<string, Data> datas, ref float[][] writeData, ref float[,] formattedData) {
            // メモリ管理気を付ける
            int index = 0;
            foreach (var paq in datas.Values) {
                writeData[index++] = paq.px.ToArray();
                writeData[index++] = paq.py.ToArray();
                writeData[index++] = paq.pz.ToArray();
                writeData[index++] = paq.qx.ToArray();
                writeData[index++] = paq.qy.ToArray();
                writeData[index++] = paq.qz.ToArray();
                writeData[index++] = paq.qw.ToArray();
            }

            // 転置する作業
            for (int i = 0; i < formattedData.GetLength(0); i++) {
                for (int j = 0; j < formattedData.GetLength(1); j++) {
                    formattedData[i, j] = writeData[j][i];
                }
            }
        }
    }

    private Dictionary<string, Data> datas;
    private List<string> csvHeaders;
    // data の数を数える
    private int dataNum = 0;

    private StreamWriter sw;

    void initializeDataDictionary() {
        foreach (var tpar in targetPositionAndRotations) {
            datas.Add(tpar.name, new Data());
        }
    }

    void Start() {
        filePath = Application.dataPath + "/data/";

        datas = new Dictionary<string, Data>();
        initializeDataDictionary();
        timeStamps = new List<float>();
        // fixedUpdate の FPS を 100Hz に
        Time.fixedDeltaTime = 0.01f;
        // CSV ファイルのヘッダー
        csvHeaders = new List<string>();

        foreach (var key in datas.Keys) {
            foreach (var suffix in Data.suffixes) {
                csvHeaders.Add(key + suffix);
            }
        }
    }

    /// <summary>
    /// 特にFPS気にしない処理は Update へ
    /// </summary>
    void Update() {

    }

    /// <summary>
    /// FPS を安定させるために FixedUpdate で記録していく
    /// </summary>
    void FixedUpdate() {
        if (isMeasuring) {
            timeStamps.Add(Time.unscaledTime);
            foreach (var tpar in targetPositionAndRotations) {
                datas[tpar.name].px.Add(tpar.transform.position.x);
                datas[tpar.name].py.Add(tpar.transform.position.y);
                datas[tpar.name].pz.Add(tpar.transform.position.z);
                datas[tpar.name].qx.Add(tpar.transform.rotation.x);
                datas[tpar.name].qy.Add(tpar.transform.rotation.y);
                datas[tpar.name].qz.Add(tpar.transform.rotation.z);
                datas[tpar.name].qw.Add(tpar.transform.rotation.w);
            }
            dataNum++;

        }

    }

    public void startLogging(string filePrefix) {
        fileName = subjectName + System.DateTime.Now.ToString("yyMMdd-HHmmss");
        sw = new StreamWriter(filePath + filePrefix + "-" + fileName + ".csv", false, Encoding.UTF8);
        isMeasuring = true;
    }

    public void endLogging() {
        isMeasuring = false;
        writeCSV();
    }


    public void writeCSV() {


        // それぞれのデータを連結して転置するという処理をしたい
        // 列ごとに並んでる方が見やすいので...
        float[][] writeData = new float[csvHeaders.Count][];
        float[,] formattedData = new float[dataNum, csvHeaders.Count];
        Data.formattingData(ref datas, ref writeData, ref formattedData);

        sw.WriteLine("timestamp, " + string.Join(",", csvHeaders));  // header の書き出し
        for (int i = 0; i < formattedData.GetLength(0); i++) {
            string line = timeStamps[i].ToString() + ",";
            for (int j = 0; j < formattedData.GetLength(1); j++) {
                line += string.Format("{0:f10},", formattedData[i, j]); // data の種類に合わせてデータの指定子(?)を変えられるように
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
