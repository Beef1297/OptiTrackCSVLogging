using System.Collections;
using System.Collections.Generic;

/// <summary>
/// CSV に書き出すデータクラス
/// TODO: 汎用的にできたらいいなぁ(たぶん無理だしやらない方がいい)
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
