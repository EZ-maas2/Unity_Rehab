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
    private bool ifClientConnected = false;

    public static event Action<Vector3> OnPosReceived;
    public static event Action<string> OnCalibrationReceived;
    public static event Action OnCalibrationOver;

    private string messageToMicro =  "1, 0, 0\n";

    public Control control_instance;

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
        ifClientConnected = true;
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
            string msg = Encoding.UTF8.GetString(buffer, 0, received);
            Debug.Log($"Received: {msg}");

            

            if (calibrationOver)
            {
            string[] receivedData = msg.Split(",");
            OnPosReceived?.Invoke(new Vector3(float.Parse(receivedData[0]), float.Parse(receivedData[1]), float.Parse(receivedData[2])));
            SendMessageToMicro(msg);

            }

            if (msg == "Over" || msg == "over"||msg == "o")
            { 
            calibrationOver = true; 
            Debug.Log("Calibration is over!--------------");
            byte[] ackMessage = Encoding.UTF8.GetBytes(messageToMicro);
            handler.Send(ackMessage);
            Debug.Log("Acknowledgment sent.");
            OnCalibrationOver?.Invoke();}

            if (calibrationOver != true) // placeholder response message for ensuring good connection during calibration
            {
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
    void ChangeMessageToMicro(string target_tag)
    {
        if (target_tag == "Front"){ messageToMicro = "1, 0, 0\n";}
        else if (target_tag == "Side"){messageToMicro = "0, 1, 0\n";}
        else if (target_tag == "Back"){messageToMicro = "0, 0, 1\n";}
        else {messageToMicro = "0, 0, 0\n";}

    }


    void SendMessageToMicro(string pos)
    {   string msg_pos;

        if (pos == "0.0, 0.0, -1.0"){ msg_pos = "Front";}
        else if (pos == "-1.0, 0.0, 0.0"){msg_pos = "Side";}
        else {msg_pos = "Center";}


        string new_pos = control_instance.getNextTargetTag();

        // if we hit the target, end the new position
        if (control_instance.currentTarget == msg_pos) {
            ChangeMessageToMicro(new_pos);
        }

        byte[] ackMessage = Encoding.UTF8.GetBytes(messageToMicro);
        handler.Send(ackMessage);
        Debug.Log("Acknowledgment sent.");


    }

    private void OnApplicationQuit()
    {
        handler?.Close();
        listener?.Close();
    }
}
