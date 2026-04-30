using UnityEngine;
using System.Collections;

public class FlyFlip : MonoBehaviour
{
    public Rigidbody playerRb;
    public float flyHeight = 2.0f;    // 上昇する高さ
    public float duration = 1.0f;     // 上昇/下降にかける時間
    public int requiredFlaps = 5;     // 必要なパタパタ回数

    private static bool touchedUpper = false;
    private static int flapCount = 0;
    private static bool isMoving = false; // 移動中に二重発動するのを防ぐ

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand") && !isMoving)
        {
            if (gameObject.name == "UpperTarget")
            {
                if (!touchedUpper)
                {
                    touchedUpper = true;
                    Debug.Log("腕が上がった！");
                }
            }
            else if (gameObject.name == "LowerTarget")
            {
                if (touchedUpper)
                {
                    flapCount++;
                    touchedUpper = false;
                    Debug.Log($"パタパタカウント: {flapCount} / {requiredFlaps}");

                    if (flapCount >= requiredFlaps)
                    {
                        Debug.Log("5回達成！飛びます！");
                        StartCoroutine(FlyRoutine());
                        flapCount = 0; // カウントリセット
                    }
                }
            }
        }
    }

    // ゆっくり上がって下がる動き
    IEnumerator FlyRoutine()
    {
        isMoving = true;
        Vector3 startPos = playerRb.position;
        Vector3 targetPos = startPos + Vector3.up * flyHeight;

        // --- 上昇 ---
        float elapsed = 0;
        while (elapsed < duration)
        {
            // Lerp（線形補間）を使って座標を滑らかに移動させる
            playerRb.MovePosition(Vector3.Lerp(startPos, targetPos, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerRb.MovePosition(targetPos);

        // 少し頂上で静止（必要なければ削ってください）
        yield return new WaitForSeconds(0.5f);

        // --- 下降 ---
        elapsed = 0;
        while (elapsed < duration)
        {
            playerRb.MovePosition(Vector3.Lerp(targetPos, startPos, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerRb.MovePosition(startPos);

        isMoving = false;
    }
}