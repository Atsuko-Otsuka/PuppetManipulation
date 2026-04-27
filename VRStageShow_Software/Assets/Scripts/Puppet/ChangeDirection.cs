using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDirection : MonoBehaviour
{
    public Transform targetTracker;
    public float rotateThreshold;
    public float neutralThreshold;

    private float lastYaw;
    private bool isReady = false;
    private float offset = 0;

    // Start is called before the first frame update
    void Start()
    {
        // 最初の角度を記録
        lastYaw = targetTracker.rotation.y;
    }

    // Update is called once per frame
    void Update()
    {
        //　傾きを取得
        Vector3 currentYaw = targetTracker.localEulerAngles;
        //Quaternion currentYaw = targetTracker.rotation;
        Debug.Log($"Yaw = {currentYaw}");

        if (isReady)
        {
            if (currentYaw.y < 150) //左回転
            {
                transform.Rotate(0, -45, 0);
                isReady = false;
            }
            else if(currentYaw.y > 210) //右回転
            {
                transform.Rotate(0, 45, 0);
                isReady = false;
            }
            
        }
        else
        {
            if (currentYaw.y > 175 && currentYaw.y < 185) // 元位置
            {
                isReady = true;
            }
        }
    }
}
