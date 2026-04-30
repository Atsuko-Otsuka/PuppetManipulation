using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KumaHeadRotationr : MonoBehaviour
{
    public SerialHandler serialHandler;
    public TMP_Text text;
    public GameObject targetObject;

    [Header("首の可動域制限 (度)")]
    // 左右の回転 (Y軸)
    [SerializeField] private float maxYaw = 80.0f;   // 右方向
    [SerializeField] private float minYaw = -80.0f;  // 左方向

    // 上下の回転 (X軸)
    [SerializeField] private float maxPitch = 60.0f; // 上方向
    [SerializeField] private float minPitch = -60.0f; // 下方向

    // 傾き (Z軸)
    [SerializeField] private float maxRoll = 40.0f;  // 右傾き
    [SerializeField] private float minRoll = -40.0f; // 左傾き

    // センサーから受信した最新の回転データ
    private Quaternion receivedQuaternion = Quaternion.identity;

    // センサーの初期回転（起動時の向きを「正面」とするためのオフセット）
    private Quaternion initialSensorRotation = Quaternion.identity;

    // 最初のデータを受信したかどうかのフラグ
    private bool isFirstDataReceived = false;

    void Start()
    {
        serialHandler.OnDataReceived += OnDataReceived;

        // 起動時の回転を初期値として保持
        receivedQuaternion = targetObject.transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
    }

    // LateUpdate は、Update や Animator の処理がすべて終わった後に呼ばれます。
    // 回転の最終調整や制限（Constraints）をかけるのに最適です。
    void LateUpdate()
    {
        // 1. センサーの初期回転からの「差分」を計算
        // (Inverse(A) * B) は、「AからBへの回転」を求めます。
        // これで、センサー起動時の向きが (0, 0, 0) の回転として扱われます。
        Quaternion relativeRotation = Quaternion.Inverse(initialSensorRotation) * receivedQuaternion;

        // 2. 差分回転をオイラー角に変換
        Vector3 relativeEuler = relativeRotation.eulerAngles;

        // 3. オイラー角を -180度～180度の範囲に正規化（クランプしやすくするため）
        float pitch = NormalizeAngle(relativeEuler.x);
        float yaw = NormalizeAngle(relativeEuler.y);
        float roll = NormalizeAngle(relativeEuler.z);

        // 4. 各軸の角度を、設定した可動域 (min/max) で制限（クランプ）
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        roll = Mathf.Clamp(roll, minRoll, maxRoll);

        // 5. 制限したオイラー角をクォータニオンに戻す
        Quaternion clampedRotation = Quaternion.Euler(pitch, yaw, roll);

        // 6. オブジェクトの「ローカル回転」として適用
        // (transform.rotation ではなく transform.localRotation を使うのが一般的です)
        targetObject.transform.localRotation = clampedRotation;
    }

    // 角度を -180 から 180 の範囲に正規化するヘルパー関数
    private float NormalizeAngle(float angle)
    {
        // 360度を超える角度を0-360の範囲に丸める (例: 370 -> 10)
        angle = angle % 360;

        // 180度より大きい角度をマイナスに変換 (例: 350 -> -10)
        if (angle > 180)
        {
            angle -= 360;
        }
        // -180度より小さい角度をプラスに変換 (例: -190 -> 170)
        else if (angle < -180)
        {
            angle += 360;
        }
        return angle;
    }

    void OnDataReceived(string message)
    {
        try
        {
            string[] angles = message.Split(',');

            float w = float.Parse(angles[0]);
            float x = float.Parse(angles[1]);
            float y = float.Parse(angles[2]);
            float z = float.Parse(angles[3]);

            // UnityのQuaternionは (x, y, z, w) の順でコンストラクタに渡す
            Quaternion arduinoQuat = new Quaternion(-x, -z, -y, w);

            text.text = "w:" + angles[0] + "x:" + angles[1] + "y:" + angles[2] + "z:" + angles[3] + "\n";

            // 最初のデータを受信したときに、それを「初期回転」として保存
            if (!isFirstDataReceived)
            {
                initialSensorRotation = arduinoQuat;
                isFirstDataReceived = true;
            }

            // 受信したデータを変数に保存するだけにする
            // 実際の適用は LateUpdate で行う
            receivedQuaternion = arduinoQuat;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }
}