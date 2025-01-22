using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class Ankle : MonoBehaviour
{
    // Ankle should handle all the logic pertaining to converting the received position into the ankle posiution 

    Vector3 curr_destination; 
    // Start is called before the first frame update
    void Start()
    {
        TcpReceiver.OnPosReceived += SetNewDestination;
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
