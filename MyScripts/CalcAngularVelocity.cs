using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 腕の方向ベクトル
/// </summary>
public class CalcAngularVelocity : MonoBehaviour {
    [SerializeField] protected Transform lShoulder;
    [SerializeField] protected Transform lHand;
    [SerializeField] protected Transform rShoulder;
    [SerializeField] protected Transform rHand;
    [SerializeField] protected Text uiText;
    [SerializeField] protected bool isExperiment = true;
    [Header("True: Flexion/Extension, False: Adduction/Abduction")]
    [SerializeField] protected bool FlEx_AdAb;

    private double lP_angle;
    private double rP_angle;

    private List<double> l_angvList;
    private List<double> r_angvList;

    private int measurementNum;

    void Start() {

        if (lShoulder == null) lShoulder = GameObject.Find("L_Shoulder").gameObject.transform;
        if (lHand == null) lHand = GameObject.Find("L_Hand").gameObject.transform;
        if (rShoulder == null) rShoulder = GameObject.Find("R_Shoulder").gameObject.transform;
        if (rHand == null) rHand = GameObject.Find("R_Hand").gameObject.transform;

        l_angvList = new List<double>();
        r_angvList = new List<double>();

    }

    // Update is called once per frame
    void FixedUpdate() {

        if (isExperiment) {
            Vector3 lArmDirection = lHand.position - lShoulder.position;
            Vector3 rArmDirection = rHand.position - rShoulder.position;
            // Flexion-Extension, Adduction-Abduction を切り替える
            // 切り替えた最初のフレームはおかしい値になるので注意(lP_angle, rP_angle のため)
            double lAngle;
            double rAngle;
            if (FlEx_AdAb) {
                lAngle = Mathf.Atan2(lArmDirection.z, lArmDirection.y) * 180 / Mathf.PI;
                rAngle = Mathf.Atan2(rArmDirection.z, rArmDirection.y) * 180 / Mathf.PI;
            } else {
                lAngle = Mathf.Atan2(lArmDirection.y, lArmDirection.x) * 180 / Mathf.PI;
                rAngle = Mathf.Atan2(rArmDirection.y, rArmDirection.x) * 180 / Mathf.PI;
            }
            //Debug.Log(angle);
            double lAngv = (lAngle - lP_angle) / Time.fixedDeltaTime;
            double rAngv = (rAngle - rP_angle) / Time.fixedDeltaTime;
            l_angvList.Add(lAngv);
            r_angvList.Add(rAngv);

            lP_angle = lAngle;
            rP_angle = rAngle;
            //m_Hand.transform.parent = m_Parent;

            // 1frame の角速度だと一瞬すぎるので，複数フレームの平均を取る
            int count = 20;
            if (l_angvList.Count > count && r_angvList.Count > count) {
                double lAverage = 0;
                double rAverage = 0;
                for (int i = 0; i < count; i++) {
                    lAverage += l_angvList[l_angvList.Count - 1 - i] / (double)count;
                    rAverage += r_angvList[r_angvList.Count - 1 - i] / (double)count;
                }
                uiText.text = string.Format("LEFT: {0:f10}\nRIGHT: {1:f10}", lAverage, rAverage);
                l_angvList.Clear();
            }
        }
    }
}
