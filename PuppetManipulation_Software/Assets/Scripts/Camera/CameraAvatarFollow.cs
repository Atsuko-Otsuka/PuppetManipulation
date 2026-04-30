using UnityEngine;

public class CameraAvatarFollow : MonoBehaviour
{
    public Transform target; // 追従対象を指定
    public float positionSmoothTime = 0.1f; // 位置の滑らかさ
    public float rotationSmoothTime = 0.1f; // 回転の滑らかさ

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // カメラのy軸は常に一定の高さを保つ
        Vector3 targetPosition = new Vector3(target.position.x, 0.4f, target.position.z);

        // 位置をスムーズに追従
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, positionSmoothTime);

        // Neckボーンの回転を取得
        float neckYaw = target.eulerAngles.y;

        // NeckボーンのY軸回転のみを反映するターゲット回転を作成
        Quaternion targetRotation = Quaternion.Euler(0, neckYaw, 0);

        // 回転をスムーズに追従
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothTime);
    }
}