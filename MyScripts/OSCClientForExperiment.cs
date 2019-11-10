using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uOSC;

public class OSCClientForExperiment : MonoBehaviour {
    uOscClient client;
    void Start() {
        client = GetComponent<uOscClient>();
    }

    public void Send<T>(string route, T message) {
        client.Send(route, message);
    }

    public void sendBundle(Bundle bundle) {
        client.Send(bundle);
    }
}
