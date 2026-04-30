using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPHandler : MonoBehaviour
{
    [SerializeField] int localPort = 9000;

    public delegate void DataReceivedDelegate(string message);
    public event DataReceivedDelegate OnDataReceived;

    private UdpClient udp;
    private Thread thread;
    private bool isRunning = false;

    void Start()
    {
        try
        {
            udp = new UdpClient(localPort);
            // タイムアウト設定
            udp.Client.ReceiveTimeout = 1000;

            isRunning = true;
            thread = new Thread(new ThreadStart(ReceiveData));
            thread.IsBackground = true;
            thread.Start();

            Debug.Log($"UDP Handler started on port {localPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"UDP Start Error: {e.Message}");
        }
    }

    private void ReceiveData()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        while (isRunning)
        {
            try
            {
                byte[] data = udp.Receive(ref remoteEP);
                string message = Encoding.ASCII.GetString(data);
                //Debug.Log($"Received UDP message from {remoteEP}: {message}");

                if (OnDataReceived != null)
                {
                    OnDataReceived(message);
                }
            }
            catch (SocketException) { /* タイムアウト時は無視して継続 */ }
            catch (Exception e) { if (isRunning) Debug.LogWarning(e.Message); }
        }
        
    }

    void OnDisable() => StopUDP();
    void OnApplicationQuit() => StopUDP();

    private void StopUDP()
    {
        isRunning = false;
        if (udp != null)
        {
            udp.Close();
            udp = null;
        }
        if (thread != null && thread.IsAlive)
        {
            thread.Join(500);
        }
    }
}