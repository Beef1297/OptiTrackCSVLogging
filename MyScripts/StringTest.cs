using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringTest : MonoBehaviour {

    /// <summary>
    /// 文字列の処理時間について調査 : csv の処理に使えるか確認するため
    /// 細かい部分は自分で調べた方が早いので実験
    /// </summary>

    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    System.Text.StringBuilder sb = new System.Text.StringBuilder();
    string s;
    string s1, s2;
    string w1 = "1.1111111";
    string w2 = "1.1111111,1.1111111,1.1111111,1.1111111,1.1111111,1.1111111,1.1111111";
    string w3 = "1.1111111,1.1111111,1.1111111,1.1111111,1.1111111,1.1111111,1.1111111," +
        "1.1111111,1.1111111,1.1111111,1.1111111,1.1111111,1.1111111,1.1111111," +
        "1.1111111,1.1111111,1.1111111,1.1111111,1.1111111,1.1111111,1.1111111," +
        "1.1111111,1.1111111,1.1111111,1.1111111,1.1111111,1.1111111,1.1111111," +
        "1.1111111,1.1111111,1.1111111,1.1111111,1.1111111,1.1111111,1.1111111";
    double totalTime = 0;
    void Start() {
        sb.Clear();
        for (int k = 0; k < 17; k++) {

            // 長い文字列を作る
            int loopnum = Mathf.FloorToInt(Mathf.Pow(2, k));
            sb.Clear();
            for (int i = 0; i < loopnum; i++) {
                sb.Append(w3);
            }

            // 何回か連結を繰り返して平均処理時間を計算
            s1 = sb.ToString();
            totalTime = 0;
            for (int i = 0; i < 10; i++) {
                sb.Clear();
                sw.Reset();
                sw.Start();
                sb.Append(s1).Append(w3);
                sw.Stop();
                totalTime += sw.Elapsed.TotalMilliseconds;
            }

            Debug.Log("1 data (" + w3.Length + " char), total:" + loopnum + " data, time:" + (totalTime / 10d) + "ms");
        }
    }


    // Update is called once per frame
    void Update() {

    }
}
