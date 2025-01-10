using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpReceiver : MonoBehaviour
{
     [Serializable]
    public struct TestStruct
    {
        public int x;
        public int y;
        public int z;
    }

    private UdpClient udpClient;
    public int listenPort = 6969; // Ensure this matches your ESP32's UDP port
    private IPEndPoint remoteEndPoint;
    public static event Action<Vector3> OnPosReceived;

   
    void Start()
    {
        udpClient = new UdpClient(listenPort);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, listenPort); // IPAddress.Any means that we need to listen to all port activity

        Debug.Log($"Listening for UDP data on port {listenPort}.");
        BeginReceive();
    }

    private void BeginReceive()
    {
        udpClient.BeginReceive(OnDataReceived, null);
    }

    private void OnDataReceived(IAsyncResult result)
    {
        try
        {
            byte[] data = udpClient.EndReceive(result, ref remoteEndPoint);
            if (data.Length > 0)
            {
                string received = Encoding.UTF8.GetString(data);
                Debug.Log($"Received data: {received}");

                string[] receivedData = received.Split(",");

                // Deserialize JSON into TestStruct
                OnPosReceived?.Invoke(new Vector3(float.Parse(receivedData[0]), float.Parse(receivedData[1]), float.Parse(receivedData[2])));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error receiving data: {e.Message}");
        }
        finally
        {
            // Continue receiving
            BeginReceive();
        }
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
    }
}
