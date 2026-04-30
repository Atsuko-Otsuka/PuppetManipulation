using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorController : MonoBehaviour
{
    // Start is called before the first frame update
    public SerialHandler serialHandler;
    public Transform CharaPos;

    int interval = 100;    // この数のフレーム数に1回だけ送信を行う
    int _count = 0;    // 回数計測用

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float pos = CharaPos.position.z*170000;
        if(pos > 0 && pos < 65535.0)
        {
            if(++_count > interval)
            {
                string str_val = pos.ToString();
                serialHandler.Write(str_val);
                _count = 0;
            }
            
        }
    }
}
