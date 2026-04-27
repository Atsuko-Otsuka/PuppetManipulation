using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltMove : MonoBehaviour
{
    [Header("ターゲット設定")]
    [Tooltip("動きを検出する対象のTransform。OptiTrackで動いているオブジェクト（アバター自身）を指定します。")]
    public Transform targetTracker;

    [Header("移動設定")]
    [Tooltip("アバターが前進する速度")]
    public float moveSpeed = 1.0f;

    //[Header("傾き検出の閾値")]
    //[Tooltip("この傾きを超えたら「傾いている」と認識するしきい値")]
    //public float tiltDetectionThreshold = 1.5f;

    // --- 内部で使用する変数 ---
    private Vector3 lastPosition;
    private float lastTiltTime;

    // 傾きを検出したか
    private bool detectedTilt = false;


    void Start()
    {
        // ターゲットが設定されていなければエラーを表示して停止
        if (targetTracker == null)
        {
            Debug.LogError("ShakeToMove: Target Trackerが設定されていません。動きを検出するオブジェクトをインスペクターから指定してください。");
            this.enabled = false;
            return;
        }

        // 初期位置をずらす(アニメーションの関係で少し浮き上がってしまうため)
        transform.Translate(0, -0.2f, 0);

        // 最初の位置を記録
        lastPosition = targetTracker.position;
    }


    void Update()
    {
        if (targetTracker == null) return;

        //　傾きを取得
        float tilt = targetTracker.localEulerAngles.x;
        //Debug.Log($"tilt = {tilt}");

        // 傾きがしきい値を超えた場合
        if (tilt > 300 && tilt < 340)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }
}