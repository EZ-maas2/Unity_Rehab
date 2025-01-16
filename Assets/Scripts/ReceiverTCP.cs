using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TcpReceiver : MonoBehaviour
{

    public int PORT = 6969;
    private Socket listener;
    private Socket handler;
    private byte[] buffer = new byte[1024];

    private void Start()
    {
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Any, PORT));
        listener.Listen(1);

        Debug.Log("Waiting for a connection...");
        listener.BeginAccept(OnClientConnected, null);
    }

    private void OnClientConnected(IAsyncResult ar)
    {
        handler = listener.EndAccept(ar);
        Debug.Log("Client connected.");
        BeginReceive();
    }

    private void BeginReceive()
    {
        if (handler != null && handler.Connected)
        {
            handler.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnDataReceived, null);
        }
    }

    private void OnDataReceived(IAsyncResult ar)
    {
        try
        {
            int received = handler.EndReceive(ar);
            string message = Encoding.UTF8.GetString(buffer, 0, received);
            Debug.Log($"Received: {message}");
            byte[] ackMessage = Encoding.UTF8.GetBytes("<|ACK|>");
            handler.Send(ackMessage);
            Debug.Log("Acknowledgment sent.");

            
        }
        catch (Exception e)
        {
            Debug.LogError($"Error receiving data: {e.Message}");
        }
        finally
        {
            // Continue receiving
            handler.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnDataReceived, null);
        }
    }

    private void OnApplicationQuit()
    {
        handler?.Close();
        listener?.Close();
    }
}
