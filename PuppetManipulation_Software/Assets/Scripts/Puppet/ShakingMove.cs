using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakingMove : MonoBehaviour
{
    [Header("ターゲット設定")]
    [Tooltip("動きを検出する対象のTransform。OptiTrackで動いているオブジェクト（アバター自身）を指定します。")]
    public Transform targetTracker;

    [Header("移動設定")]
    [Tooltip("アバターが前進する速度")]
    public float moveSpeed = 1.0f;

    [Header("シェイク検出の感度設定")]
    [Tooltip("この速度（m/s）を超えたら「振られた」と認識するY軸速度のしきい値")]
    public float shakeDetectionThreshold = 1.5f;

    [Tooltip("上下の振りがこの秒数以内に行われたら「1回のシェイク」と判定します")]
    public float shakeDetectionInterval = 0.5f;

    // --- 内部で使用する変数 ---
    private Vector3 lastPosition;
    private float lastShakeTime;

    // 上方向へのシェイクを検出したか
    private bool detectedUpShake = false;
    // 下方向へのシェイクを検出したか
    private bool detectedDownShake = false;


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
        transform.Translate(0,-0.2f,0);

        // 最初の位置を記録
        lastPosition = targetTracker.position;
    }


    void Update()
    {
        if (targetTracker == null) return;

        // --- 速度の計算 ---
        // 現在位置と前のフレームの位置との差分から、1秒あたりの移動速度を計算します
        float velocityY = (targetTracker.position.y - lastPosition.y) / Time.deltaTime;

        // --- シェイクの検出 ---
        // 上方向の速度がしきい値を超えた場合
        if (velocityY > shakeDetectionThreshold)
        {
            detectedUpShake = true;
            lastShakeTime = Time.time; // 最後に振られた時刻を記録
        }

        // 下方向の速度がしきい値を超えた場合
        if (velocityY < -shakeDetectionThreshold)
        {
            detectedDownShake = true;
            lastShakeTime = Time.time; // 最後に振られた時刻を記録
        }

        // --- 1回のシェイク完了と前進処理 ---
        // 上方向と下方向の両方の振りが検出されたら
        if (detectedUpShake && detectedDownShake)
        {
            // アバターをその向いている方向に前進させる
            // Vector3.forward はオブジェクトのローカル座標での前方を示す
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

            // 検出フラグをリセットして、次のシェイクに備える
            ResetShakeDetection();
        }

        // --- タイムアウト処理 ---
        // 最後に振られてから一定時間が経過したら、片方だけの検出フラグをリセットする
        // (例: 上に振っただけで終わった場合など)
        if (Time.time - lastShakeTime > shakeDetectionInterval)
        {
            ResetShakeDetection();
        }

        // --- 次のフレームのための位置更新 ---
        // 現在の位置を「前のフレームの位置」として保存する
        lastPosition = targetTracker.position;
    }

    /// <summary>
    /// シェイクの検出状態をリセットします
    /// </summary>
    private void ResetShakeDetection()
    {
        detectedUpShake = false;
        detectedDownShake = false;
    }
}