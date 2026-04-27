using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPCameraController : MonoBehaviour
{
    [SerializeField] GameObject player;        // 追従対象

    [SerializeField] float height = 1f;  // カメラの高さ

    [Header("追従スムーズ係数")]
    [SerializeField] float posSmooth = 0.15f;
    [SerializeField] float rotSmooth = 8f;

    Vector3 targetPos;

    void Update()
    {
        /* 位置――頭（目）の座標に固定 */
        Vector3 localOffset = new Vector3(0, height, 0) + Vector3.forward * 0.3f;   // 目線の高さ
        transform.position = player.transform.TransformPoint(localOffset);

        /* 回転――プレイヤーと同じ向きへ */
        Quaternion targetRot = player.transform.rotation;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotSmooth);       
    }
}