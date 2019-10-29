using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 腕の方向ベクトル
/// </summary>
public class CalcAngularVelocity : MonoBehaviour {
    [SerializeField] protected GameObject t_Shoulder;
    [SerializeField] protected Text uiText;
    private GameObject m_Hand; // 通常はこのコンポーネントを付与したオブジェクト

    private Transform m_Parent;
    private double p_angle;
    private List<double> angv_list;

    private int measurementNum;

    void Start() {


        angv_list = new List<double>();
        m_Hand = this.gameObject;
        Time.fixedDeltaTime = 0.00833333333f; // 120FPS に

    }

    // Update is called once per frame
    void FixedUpdate() {

        Vector3 armDirection = m_Hand.transform.position - t_Shoulder.transform.position;
        //double angle = Mathf.Atan2(m_Hand.transform.localPosition.y, m_Hand.transform.localPosition.z) * 180 / Mathf.PI;
        double angle = Mathf.Atan2(armDirection.z, armDirection.y) * 180 / Mathf.PI;
        //Debug.Log(angle);
        double ang_v = (angle - p_angle) / Time.fixedDeltaTime;
        angv_list.Add(ang_v);

        p_angle = angle;
        //m_Hand.transform.parent = m_Parent;

        // 1frame の角速度だと一瞬すぎるので，複数フレームの平均を取る
        int count = 20;
        if (angv_list.Count > count) {
            double average = 0;
            for (int i = 0; i < count; i++) {
                average += angv_list[angv_list.Count - 1 - i] / (double)count;
            }
            uiText.text = average.ToString();
            angv_list.Clear();
        }
    }
}
