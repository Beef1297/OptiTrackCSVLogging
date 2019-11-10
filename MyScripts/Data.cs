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

}
