using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    public int listenPort = 12345; // Ensure this matches your ESP32's UDP port
    private IPEndPoint remoteEndPoint;

    [Serializable]
    public struct TestStruct
    {
        public int x;
        public int y;
    }

    void Start()
    {
        udpClient = new UdpClient(listenPort);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, listenPort);

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
                string jsonData = Encoding.UTF8.GetString(data);
                Debug.Log($"Received data: {jsonData}");

                // Deserialize JSON into TestStruct
                TestStruct receivedData = JsonUtility.FromJson<TestStruct>(jsonData);
                Debug.Log($"Received Struct: x = {receivedData.x}, y = {receivedData.y}");
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
