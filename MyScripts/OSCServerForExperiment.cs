using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uOSC;

public class OSCServerForExperiment : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        var server = GetComponent<uOscServer>();
    }

    void OnDataReceived(Message message) {
        var msg = message.address + ": ";

    }
}
