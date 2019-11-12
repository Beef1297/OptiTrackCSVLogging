using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uOSC;
using System.IO;
using System.Text;
using System.Linq;

public class ExperimentController : MonoBehaviour {
    [SerializeField] public OptiCSVLogging csvlogger;
    [SerializeField] public OSCClientForExperiment oscclient;
    [Header("被験者名")]
    [SerializeField] private string subjectName;
    [Header("実験1角速度順番")]
    [SerializeField] string[] ex1_order;
    [Header("実験2振動パターンファイルパス")]
    [SerializeField] public string ex2PatternFile;
    [Header("練習振動パターンファイルパス")]
    [SerializeField] public string practicePatternFile;
    Dictionary<string, string> oscStates = new Dictionary<string, string>
    {
        { "start", "/start" },
        { "stop", "/stop" },
    };

    private Dictionary<string, List<int>> ex2VibrationPatterns;
    private Dictionary<string, List<int>> practiceVibrationPatterns;


    // 評価
    private List<int> vividness;
    private List<int> duration;
    private List<int> magnitude;


    // 実験1, 実験2の最中は true になる
    private bool isMeasuring = false;

    [Header("音のパラメータ")]
    [SerializeField] private float whiteNoiseVol = 1.0f;
    [SerializeField] private float startSEVol = 0.5f;
    [SerializeField] private float[] vibrationVol;

    // 測定回数
    private int measurementNum;
    [Header("測定回数")]
    [SerializeField] private int ex1MeasurementNum = 6;
    [SerializeField] private int ex2MeasurementNum = 3;
    [SerializeField] private int practiceNum = 10;

    // 時間管理
    [Header("時間管理")]
    [SerializeField] private float ex1_measureTime = 5.0f;
    [SerializeField] private float vibrationTime = 5.0f;
    [SerializeField] private float breakingTime = 5.0f;
    [SerializeField] private float largeBreakingTime = 60.0f;


    void setVibrationPattern(string _path, ref Dictionary<string, List<int>> vP) {
        StreamReader sr = new StreamReader(_path, Encoding.UTF8);

        string[] patternHeaders = sr.ReadLine().Split(',');
        for (int i = 0; i < patternHeaders.Length; i++) {
            if (string.IsNullOrEmpty(patternHeaders[i])) continue;
            vP.Add(patternHeaders[i], new List<int>());
        }

        while (sr.Peek() > -1) {
            string[] cols = sr.ReadLine().Split(',');
            foreach (var item in vP.Values.Select((value, index) => new { value, index })) {
                item.value.Add(System.Int32.Parse(cols[item.index]));
            }
        }
        sr.Close();
    }

    void Start() {
        if (csvlogger == null) {
            csvlogger = GameObject.Find("OptiCSVLogger").GetComponent<OptiCSVLogging>();
        }
        if (oscclient == null) {
            oscclient = GameObject.Find("OSCClient").GetComponent<OSCClientForExperiment>();
        }

        if (string.IsNullOrEmpty(subjectName)) {
            subjectName = "test";
        }

        if (vibrationVol.Length < 7) {
            Debug.LogError("振動子7つのボリュームを設定してください");
            Application.Quit();
        }

        if (string.IsNullOrEmpty(ex2PatternFile)) {
            Debug.LogError("pattern file is empty!!");
            Application.Quit();
        }
        if (ex1_order.Length < ex1MeasurementNum) {
            Debug.LogError("【実験1】測定回数分の測定順序を入力してください");
        }
        Debug.Log("experiment2 pattern file is: " + ex2PatternFile);

        // 1行目はヘッダなので，それを利用して辞書型配列作成
        ex2VibrationPatterns = new Dictionary<string, List<int>>();
        setVibrationPattern(ex2PatternFile, ref ex2VibrationPatterns);

        practiceVibrationPatterns = new Dictionary<string, List<int>>();
        setVibrationPattern(practicePatternFile, ref practiceVibrationPatterns);

        vividness = new List<int>();
        duration = new List<int>();
        magnitude = new List<int>();

        measurementNum = 1;
    }


    Bundle bundleVolumePattern(Dictionary<string, List<int>> vP) {
        Bundle bundle = new Bundle(Timestamp.Now);
        bundle.Add(new Message("/vibrators/1", vP["vibrator1"][measurementNum - 1] * vibrationVol[0]));
        bundle.Add(new Message("/vibrators/2", vP["vibrator2"][measurementNum - 1] * vibrationVol[1]));
        bundle.Add(new Message("/vibrators/3", vP["vibrator3"][measurementNum - 1] * vibrationVol[2]));
        bundle.Add(new Message("/vibrators/4", vP["vibrator4"][measurementNum - 1] * vibrationVol[3]));
        bundle.Add(new Message("/vibrators/5", vP["vibrator5"][measurementNum - 1] * vibrationVol[4]));
        bundle.Add(new Message("/vibrators/6", vP["vibrator6"][measurementNum - 1] * vibrationVol[5]));
        bundle.Add(new Message("/vibrators/7", vP["vibrator7"][measurementNum - 1] * vibrationVol[6]));
        return bundle;
    }

    Bundle bundleResetVolume() {
        Bundle bundle = new Bundle(Timestamp.Now);
        bundle.Add(new Message("/vibrators/1", 0));
        bundle.Add(new Message("/vibrators/2", 0));
        bundle.Add(new Message("/vibrators/3", 0));
        bundle.Add(new Message("/vibrators/4", 0));
        bundle.Add(new Message("/vibrators/5", 0));
        bundle.Add(new Message("/vibrators/6", 0));
        bundle.Add(new Message("/vibrators/7", 0));
        return bundle;
    }

    IEnumerator experiment1() {
        Debug.Log("start experiment1");
        isMeasuring = true;

        while (measurementNum <= ex1MeasurementNum) {
            Debug.Log("第 " + measurementNum + " 測定");
            Debug.Log("To measure, please enter keypadenter or m");
            yield return new WaitUntil(() => {
                return Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.M);
            });
            Debug.Log("logging...");
            csvlogger.startLogging(subjectName, string.Format("ex1-{0}degs-{1}", ex1_order[measurementNum - 1], measurementNum));
            yield return new WaitForSeconds(ex1_measureTime);
            csvlogger.endLogging();
            measurementNum++;
        }

        Debug.Log("end experiment1");
        isMeasuring = false;
    }



    IEnumerator experiment2(bool is_experiment) {
        if (is_experiment) {
            Debug.Log("start experiment2");
        } else {
            Debug.Log("start practice");
        }
        isMeasuring = true;

        int maxNum = (is_experiment) ? ex2MeasurementNum : practiceNum;

        while (measurementNum <= maxNum) {
            // vibration start phase
            oscclient.Send("/start", 1);
            // これから振動するという効果音を鳴らす
            oscclient.Send("/sound_vol", startSEVol);
            oscclient.Send("/sound_channel", 2);
            yield return new WaitForSeconds(2.0f); // 実験姿勢を作ってもらうために数秒おく

            Debug.Log("vibration phase");
            oscclient.Send("/sound_vol", whiteNoiseVol); // ホワイトノイズの Vol 設定
            oscclient.Send("/sound_channel", 1); // 振動の 少し前からホワイトノイズを流し始める
            // 意図しない錯覚が生起した際にメモするため
            Debug.Log(string.Format("{0} : 振動パターンは{1}です", measurementNum, ex2VibrationPatterns["patternID"][measurementNum - 1]));
            yield return new WaitForSeconds(1.0f); // 1秒前からホワイトノイズを流す
            // 本番なら csv log 開始
            oscclient.sendBundle(bundleVolumePattern(ex2VibrationPatterns)); // 振動パターンを送る
            if (is_experiment) csvlogger.startLogging(subjectName, "ex2-" + ex2VibrationPatterns["patternID"][measurementNum - 1] + "-" + measurementNum);
            // 振動開始
            yield return new WaitForSeconds(vibrationTime); // vibrationTime だけ止めることで振動を提示する

            // breaking (vibration end phase)
            oscclient.Send(oscStates["stop"], 0); // 振動停止
            oscclient.sendBundle(bundleResetVolume()); // 振動子のvolume を reset する
            oscclient.Send("/sound_channel", 0); // ホワイトノイズ停止
            // 本番ならcsv log 停止
            if (is_experiment) csvlogger.endLogging();

            // 被験者が主観評価を回答するまで待つ
            Debug.Log("Waiting Answer: if finished, put tenkey enter or right arrow");
            yield return new WaitUntil(() => {
                return Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.RightArrow);
            });

            // 10 回ごとに大きい休憩を取る
            if (measurementNum % 10 == 0) {
                Debug.Log("large braking phase");
                yield return new WaitForSeconds(largeBreakingTime);
                Debug.Log("end break, please prepare for next vibration: put keypadenter or rightarrow");
                yield return new WaitUntil(() => {
                    return Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.RightArrow);
                });
            } else {
                Debug.Log("braking phase");
                yield return new WaitForSeconds(breakingTime);
            }
            measurementNum++;
        }

        if (is_experiment) {
            Debug.Log("end experiment2");
        } else {
            Debug.Log("end practice");
        }
        isMeasuring = false;
    }


    void Update() {
        if (!isMeasuring && Input.GetKeyDown(KeyCode.Alpha1)) {
            measurementNum = 1;
            StartCoroutine("experiment1");
        }

        if (!isMeasuring && Input.GetKeyDown(KeyCode.Alpha2)) {
            measurementNum = 1;
            StartCoroutine("experiment2", true);
        }

        if (!isMeasuring && Input.GetKeyDown(KeyCode.P)) {
            measurementNum = 1;
            StartCoroutine("experiment2", false);
        }
    }

}
