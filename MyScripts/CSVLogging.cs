using System.Collections;
using System.Collections.Generic;

using System.IO; // StreamWriter
using UnityEngine;

public abstract class CSVLogging : MonoBehaviour {
    protected string filePath; // Start で [Unity Project Path]/Assets/data/ になる
    protected string fileName; // Controller から 指定される．
    protected string filePrefix; // Controller から指定される
    protected StreamWriter sw;
    virtual protected void Start() {
        filePath = Application.dataPath + "/data/";
    }

    abstract public void startLogging(string subjectName, string _filePrefix);
    abstract public void endLogging();
    abstract protected void writeCSV();
}