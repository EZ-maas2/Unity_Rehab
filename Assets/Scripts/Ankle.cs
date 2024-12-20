using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class Ankle : MonoBehaviour
{

    Vector3 curr_destination; 
    // Start is called before the first frame update
    void Start()
    {
        UdpReceiver.OnPosReceived += SetNewDestination;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = curr_destination;
       // curr_destination += Vector3.forward * 0.001f; // test purposes
    }

    void SetNewDestination(Vector3 pos)
    { curr_destination = pos;
    }
}
