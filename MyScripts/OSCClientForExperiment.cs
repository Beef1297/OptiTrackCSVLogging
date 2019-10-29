using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uOSC;

public class OSCClientForExperiment : MonoBehaviour {
    // Start is called before the first frame update
    uOscClient client;
    void Start() {
        client = GetComponent<uOscClient>();
    }

    public void sendMessage(string route, string message) {
        client.Send(route, message);
    }

    public void sendInt(string route, int value) {
        client.Send(route, value);
    }

    public void sendBundle(Bundle bundle) {
        client.Send(bundle);
    }
}
