using UnityEngine;

public class FirstPCamera2 : MonoBehaviour
{
    [Header("追従対象")]
    [SerializeField] Transform playerRoot;      // Avatar のルート

    [Header("オフセット")]
    [SerializeField] float eyeHeight = 1.55f;  // 目線の高さ Y
    [SerializeField] float forwardOffset = 0.15f;  // 体の前に出す距離 Z（+なら前）

    [Header("スムーズ係数")]
    [SerializeField] float posSmooth = 10f;     // 値が大きいほど速く追従
    [SerializeField] float rotSmooth = 15f;

    Vector3 velocity = Vector3.zero;            // SmoothDamp 用

    void LateUpdate()                           // アニメーション後に実行
    {
        if (!playerRoot) return;

        /* 1. 目線の基準点（頭の中心）を求める */
        Vector3 localHead = new(0f, eyeHeight, 0f);

        /* 2. さらに forward 方向へオフセット */
        Vector3 localOffset = localHead + Vector3.forward * forwardOffset;

        /* 3. ワールド座標へ変換 */
        Vector3 desiredPos = playerRoot.TransformPoint(localOffset);

        /* 4. 位置をスムーズに反映 */
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref velocity,
            1f / posSmooth);        // 時定数の逆数イメージ

        /* 5. 回転（向き）も補間して合わせる */
        Quaternion desiredRot = playerRoot.rotation;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRot,
            Time.deltaTime * rotSmooth);
    }
}
