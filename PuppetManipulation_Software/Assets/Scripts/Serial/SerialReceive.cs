using UnityEngine;

public class SerialReceive : MonoBehaviour
{
    public SerialHandler serialHandler;
    public Vector3 currentAccel = Vector3.zero;

    void Start()
    {
        if (serialHandler != null)
        {
            serialHandler.OnDataReceived += OnDataReceived;
        }
        else
        {
            Debug.LogError("[SerialReceive] SerialHandler not assigned in Inspector!");
        }
    }

    void OnDataReceived(string message)
    {
        string[] values = message.Split(',');
        if (values.Length < 3) return;

        float ax, ay, az;
        if (float.TryParse(values[0], out ax) &&
            float.TryParse(values[1], out ay) &&
            float.TryParse(values[2], out az))
        {
            currentAccel = new Vector3(ax, ay, az);
        }

        Debug.Log("[SerialReceive] Received: " + currentAccel);
    }
}