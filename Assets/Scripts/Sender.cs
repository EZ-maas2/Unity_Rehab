using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Sender : MonoBehaviour
{
    private GameObject currTargetObj;
    public GameObject ankleObj;
    public GameObject centerObj;

    private double distance = -1.0;


    private UdpClient udpClient;
    public int PORT = 4242; // Ensure this matches your ESP32's UDP port

    private string IP = "127.0.0.1";
    private IPEndPoint remoteEndPoint;



    // public string GetLocalIPAddress()
    // {
    //     try
    //     {
    //         // Get the host name of the current device
    //         string hostName = Dns.GetHostName();

    //         // Get the list of IP addresses associated with the device
    //         IPAddress[] addresses = Dns.GetHostAddresses(hostName);

    //         foreach (IPAddress address in addresses)
    //         {
    //             // Select the first IPv4 address that isn't a loopback
    //             if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    //             {
    //                 return address.ToString();
    //             }
    //         }

    //         return "No IPv4 address found.";
    //     }

    //     catch (System.Exception ex)
    //     {
    //         Debug.LogError($"Error getting local IP: {ex.Message}");
    //         return "Error retrieving IP address.";
    //     }
    // }


    // Start is called before the first frame update
    void Start()
    {
        Control.OnCurrTargetChanged += SetCurrTarget;
        udpClient = new UdpClient();
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), PORT); 

        Debug.Log($"Sending UDP data on port {PORT}.");
        
    }

    void SendMessage()
    {
        try
        {
        string message = distance.ToString();
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, remoteEndPoint);
        }

        catch (System.Exception e)
        {
            Debug.LogError($"Error sending data: {e.Message}");
        }
    }


    // Update is called once per frame
    void Update()
    {
        CalculateDistance();
        SendMessage();
    }

    void SetCurrTarget(GameObject target)
    {
        currTargetObj = target;
    }


    // Calculate the distance from currTarget to ankle object (in 2D)

    void CalculateDistance()
    {
        float z_t = (currTargetObj.transform.position.z - centerObj.transform.position.z)/2;
        float x_t = (currTargetObj.transform.position.x - centerObj.transform.position.x)/2;
        distance = Mathf.Sqrt(Mathf.Pow(ankleObj.transform.position.x - x_t, 2) + Mathf.Pow(ankleObj.transform.position.z - z_t, 2));

    }

  void OnApplicationQuit()
{
    if (udpClient != null)
    {
        udpClient.Close();
    }

    Control.OnCurrTargetChanged -= SetCurrTarget; 
}


}
