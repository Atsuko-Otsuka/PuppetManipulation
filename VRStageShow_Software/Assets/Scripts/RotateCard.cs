using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCard : MonoBehaviour
{
    float mode_time = 0.0f;
    void Start()
    {
       
    }

    void Update()
    {
        // カードの回転が1秒ごとに切り替わる
        mode_time += Time.deltaTime;
        if (mode_time < 1.0f) this.transform.rotation = Quaternion.Euler(90, 0, 180);
        else if (mode_time < 2.0f) this.transform.rotation = Quaternion.Euler(90, 0, 0);
        else mode_time = 0.0f;
    }
}
