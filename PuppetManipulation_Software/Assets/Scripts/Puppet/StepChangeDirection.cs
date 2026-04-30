using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepChangeDirection : MonoBehaviour
{
    public Transform targetTracker;      // ぬいぐるみをトラッキングする Transform
    public float neutralMargin = 30f;     // ニュートラルとみなす ±角度 [deg]
    public float stepPerFrame = 1f;      // 1 フレームあたりに回す角度 [deg]

    // Update は毎フレーム呼ばれる
    void Update()
    {
        // ぬいぐるみのヨー角（0-360°）を取得
        float yaw = targetTracker.localEulerAngles.y;

        /* 180°＝正面とし、そこからの差分（-180°〜+180°）を求める */
        // OptiTrackの時
        //float delta = Mathf.DeltaAngle(yaw, 180f);   // 正面より左なら負、右なら正
        // Arduino + IMU の時
        float delta = Mathf.DeltaAngle(yaw, 0f);     // 正面より左なら負、右なら正

        //Debug.Log($"Yaw = {yaw}, {delta}");
        /* ───────────────────────
           左右に傾けている間だけ 1° ずつ回転させる
           neutralMargin 内なら停止
           ─────────────────────── */
        if (delta < -neutralMargin)          // 左に傾けている
        {
            // transform.Rotate(0, -stepPerFrame, 0);      // フレーム依存
            transform.Rotate(0, stepPerFrame * Time.deltaTime * 60f, 0); // 秒間 60° 相当
        }
        else if (delta > neutralMargin)      // 右に傾けている
        {
            // transform.Rotate(0,  stepPerFrame, 0);      // フレーム依存
            transform.Rotate(0, -stepPerFrame * Time.deltaTime * 60f, 0);  // 秒間 60° 相当
        }
        /* neutralMargin の範囲内では何もしない＝回転停止 */
    }
}
