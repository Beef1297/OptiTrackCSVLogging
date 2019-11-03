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
    [SerializeField] public string patternFile;
    private AudioSource whiteNoise;
    Dictionary<string, string> oscStates = new Dictionary<string, string>
    {
        { "start", "/start" },
        { "stop", "/stop" },
    };

    private Dictionary<string, List<int>> vibrationPatterns;


    // 評価
    private List<int> vividness;
    private List<int> duration;
    private List<int> magnitude;

    // 測定回数
    private int measurementNum;
    static private int ex1MeasurementNum = 3;
    static private int ex2MeasurementNum = 3;

    // 時間管理
    private float vibrationTime = 5.0f;
    private float breakingTime = 5.0f;


    void Start() {
        if (csvlogger == null) {
            csvlogger = GameObject.Find("OptiCSVLogger").GetComponent<OptiCSVLogging>();
        }
        if (oscclient == null) {
            oscclient = GameObject.Find("OSCClient").GetComponent<OSCClientForExperiment>();
        }

        if (string.IsNullOrEmpty(patternFile)) {
            Debug.LogError("pattern file is empty!!");
        }
        Debug.Log("pattern file is: " + patternFile);

        StreamReader sr = new StreamReader(patternFile, Encoding.UTF8);

        // 1行目はヘッダなので，それを利用して辞書型配列作成
        vibrationPatterns = new Dictionary<string, List<int>>();
        string[] patternHeaders = sr.ReadLine().Split(',');
        for (int i = 0; i < patternHeaders.Length; i++) {
            if (string.IsNullOrEmpty(patternHeaders[i])) continue;
            vibrationPatterns.Add(patternHeaders[i], new List<int>());
        }


        while (sr.Peek() > -1) {
            string[] cols = sr.ReadLine().Split(',');
            foreach (var item in vibrationPatterns.Values.Select((value, index) => new { value, index })) {
                item.value.Add(System.Int32.Parse(cols[item.index]));
            }

        }

        Debug.Log(string.Join(",", vibrationPatterns.Keys.ToArray()));
        vividness = new List<int>();
        duration = new List<int>();
        magnitude = new List<int>();

        measurementNum = 1;
        whiteNoise = GetComponent<AudioSource>();
    }


    Bundle formattingBudle() {
        Bundle bundle = new Bundle(Timestamp.Now);
        bundle.Add(new Message("/vibrators/1", vibrationPatterns["vibrator1"][measurementNum - 1]));
        bundle.Add(new Message("/vibrators/2", vibrationPatterns["vibrator2"][measurementNum - 1]));
        bundle.Add(new Message("/vibrators/3", vibrationPatterns["vibrator3"][measurementNum - 1]));
        bundle.Add(new Message("/vibrators/4", vibrationPatterns["vibrator4"][measurementNum - 1]));
        bundle.Add(new Message("/vibrators/5", vibrationPatterns["vibrator5"][measurementNum - 1]));
        bundle.Add(new Message("/vibrators/6", vibrationPatterns["vibrator6"][measurementNum - 1]));
        bundle.Add(new Message("/vibrators/7", vibrationPatterns["vibrator7"][measurementNum - 1]));
        return bundle;
    }

    IEnumerator experiment1() {

        Debug.Log("start experiment1");
        while (measurementNum <= ex1MeasurementNum) {
            Debug.Log("To measure, please enter keypadenter or m");
            yield return new WaitUntil(() =>
            {
                return Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.M);
            });
            Debug.Log("logging...");
            csvlogger.startLogging("ex1-" + measurementNum);
            yield return new WaitForSeconds(5.0f);
            csvlogger.endLogging();
            measurementNum++;
        }
        Debug.Log("end experiment1");
    }

    IEnumerator experiment2() {
        Debug.Log("start experiment2");
        while (measurementNum <= ex2MeasurementNum) {
            // vibration start phase
            Debug.Log("vibration phase");
            oscclient.sendBundle(formattingBudle());
            //oscclient.sendInt("/headphone", 1);
            whiteNoise.Play();
            yield return new WaitForSeconds(0.3f);
            oscclient.sendInt(oscStates["start"], 1);
            csvlogger.startLogging("ex2-" + vibrationPatterns["patternID"][measurementNum - 1] + "-" + measurementNum);
            yield return new WaitForSeconds(vibrationTime);

            // breaking (vibration end phase)
            oscclient.sendInt(oscStates["stop"], 0);
            //oscclient.sendInt("/headphone", 0);
            whiteNoise.Stop();
            csvlogger.endLogging();

            Debug.Log("Waiting Answer: if finished, put tenkey enter or right arrow");
            yield return new WaitUntil(() =>
            {
                return Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.RightArrow);
            });
            Debug.Log("braking phase");
            yield return new WaitForSeconds(breakingTime);
            measurementNum++;
        }
        Debug.Log("end experiment2");
    }


    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            measurementNum = 1;
            StartCoroutine("experiment1");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            measurementNum = 1;
            StartCoroutine("experiment2");
        }
    }

}
