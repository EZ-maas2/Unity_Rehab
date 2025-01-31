using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TcpReceiver : MonoBehaviour
{

    public int PORT = 6969;
    private Socket listener;
    private Socket handler;
    private byte[] buffer = new byte[1024];
    private bool calibrationOver = false;

    public static event Action<Vector3> OnPosReceived;
    public static event Action<string> OnCalibrationReceived;
    public static event Action OnCalibrationOver;

    private string messageToMicro =  "0, 0, 0";

    private void Start()
    {
        Control.OnCurrTargetChanged += ChangeMessageToMicro;
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

    //void()

    private void OnDataReceived(IAsyncResult ar)
    {
        try
        {
            int received = handler.EndReceive(ar);
            string msg = Encoding.UTF8.GetString(buffer, 0, received);
            Debug.Log($"Received: {msg}");

            

            if (calibrationOver)
            {
            OnCalibrationOver?.Invoke();
            string[] receivedData = msg.Split(",");
            OnPosReceived?.Invoke(new Vector3(float.Parse(receivedData[0]), float.Parse(receivedData[1]), float.Parse(receivedData[2])));

            byte[] ackMessage = Encoding.UTF8.GetBytes(messageToMicro);
            handler.Send(ackMessage);
            Debug.Log("Acknowledgment sent.");

            }

            if (msg == "Over" || msg == "over"||msg == "o")
            { calibrationOver = true; 
            Debug.Log("Calibration is over!--------------");
            byte[] ackMessage = Encoding.UTF8.GetBytes("Calibration ended");
            handler.Send(ackMessage);
            Debug.Log("Acknowledgment sent.");}

            if (calibrationOver != true) {
                byte[] ackMessage = Encoding.UTF8.GetBytes("Got it");
                handler.Send(ackMessage);
                Debug.Log("Acknowledgment sent.");
                CalibrationMsg(msg);
                
            }

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

    void CalibrationMsg(string msg)
    {
        if (msg == "Front" || msg == "Side" || msg == "Center"){
            OnCalibrationReceived(msg);
        }

    }
    void ChangeMessageToMicro(GameObject targetObj)
    {
        string target_tag = targetObj.tag;
        if (target_tag == "Front"){ messageToMicro = "1, 0, 0";}
        else if (target_tag == "Side"){messageToMicro = "0, 1, 0";}
        else if (target_tag == "Back"){messageToMicro = "0, 0, 1";}
        else {messageToMicro = "0, 0, 0";}

    }

    private void OnApplicationQuit()
    {
        handler?.Close();
        listener?.Close();
    }
}
