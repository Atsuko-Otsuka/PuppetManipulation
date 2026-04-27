using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JointWorldRotation : MonoBehaviour
{
    //public SerialHandler serialHandler;
    public UDPHandler udpHandler;
    //public TMP_Text text;
    public GameObject targetObject;

    [Header("可動域制限 (度)")]
    // 左右の回転 (Y軸)
    [SerializeField] private float maxYaw = 180.0f;   // 右方向
    [SerializeField] private float minYaw = -180.0f;  // 左方向

    // 上下の回転 (X軸)
    [SerializeField] private float maxPitch = 180.0f; // 上方向
    [SerializeField] private float minPitch = -180.0f; // 下方向

    // 傾き (Z軸)
    [SerializeField] private float maxRoll = 180.0f;  // 右傾き
    [SerializeField] private float minRoll = -180.0f; // 左傾き

    public int targetPort = 0;

    // Quarternionに渡す回転軸の順番を指定する
    [Header("Quaternion軸マッピング (1=x, 2=y, 3=z, 負の数で反転)")]
    public int quatX = 1;
    public int quatY = 2;
    public int quatZ = 3;

    public KeyCode calibrationKey = KeyCode.R;

    [Header("オフセット調整")] // ★追加: 手動でひねりを修正する場合に使用
    public Vector3 offsetEuler = Vector3.zero;

    [Header("スパイク除去")]
    [Range(10.0f, 360.0f)]
    public float thresholdAngle = 360.0f;

    [Header("Smoothing")]
    [Range(1f, 20f)]
    public float smoothingFactor = 10.0f;

    // センサーから受信した最新の回転データ
    private Quaternion receivedQuaternion = Quaternion.identity;

    // センサーの初期回転（起動時の向きを「正面」とするためのオフセット）
    private Quaternion initialSensorRotation = Quaternion.identity;

    // ★追加: Unity上のオブジェクト（手首）の初期回転
    private Quaternion initialTargetRotation = Quaternion.identity;

    // 最初のデータを受信したかどうかのフラグ
    private bool isFirstDataReceived = false;

    void Start()
    {
        //serialHandler.OnDataReceived += OnDataReceived;
        udpHandler.OnDataReceived += OnDataReceived;

        // 起動時の回転を初期値として保持
        // receivedQuaternion = targetObject.transform.localRotation; 

        // ゲーム開始時のUnity上のボーンの角度を保存しておく
        if (targetObject != null)
        {
            //initialTargetRotation = targetObject.transform.localRotation;
            initialTargetRotation = targetObject.transform.rotation;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(calibrationKey))
        {
            CalibrateSensor();
        }
    }

    public void CalibrateSensor()
    {
        // 既存の+= OnDataReceivedは削除（Startで一度登録すればOK）
        initialSensorRotation = receivedQuaternion;
        Debug.Log($"[Port {targetPort}] キャリブレーション完了");
    }

    void LateUpdate()
    {
        if (!isFirstDataReceived) return; 

        // 1. センサーの初期回転からの「差分」を計算
        // これにより、センサーがどの向きで始まっても、そこからの「変化量」が取れる
        Quaternion relativeRotation = Quaternion.Inverse(initialSensorRotation) * receivedQuaternion;

        // 2. 差分回転をオイラー角に変換
        Vector3 relativeEuler = relativeRotation.eulerAngles;

        // 3. オイラー角を -180度～180度の範囲に正規化
        float pitch = NormalizeAngle(relativeEuler.x);
        float yaw = NormalizeAngle(relativeEuler.y);
        float roll = NormalizeAngle(relativeEuler.z);

        // 4. 各軸の角度を制限（クランプ）
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        roll = Mathf.Clamp(roll, minRoll, maxRoll);

        // 5. 制限したオイラー角をクォータニオンに戻す（これがセンサーによる「動き」成分）
        Quaternion clampedRotation = Quaternion.Euler(pitch, yaw, roll);

        // 最終的な回転を計算
        // 「Unityの初期回転」 × 「センサーの動き」 × 「手動オフセット」
        Quaternion finalGoalRotation = initialTargetRotation * clampedRotation * Quaternion.Euler(offsetEuler);

        // 6. 適用
        //targetObject.transform.localRotation = Quaternion.Slerp(
        //    targetObject.transform.localRotation,
        //    finalGoalRotation,
        //    Time.deltaTime * smoothingFactor
        //);
        targetObject.transform.rotation = Quaternion.Slerp(
        targetObject.transform.rotation, // 現在のワールド回転
        finalGoalRotation,               // 目標のワールド回転
        Time.deltaTime * smoothingFactor
        );
    }

    // --- (以下、NormalizeAngle, GetMappedAxis, OnDataReceived は変更なしのため省略可能です) ---
    // コピーペースト用に以下に残しておきます

    private float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        if (angle > 180) angle -= 360;
        else if (angle < -180) angle += 360;
        return angle;
    }

    private float GetMappedAxis(int mapping, Quaternion quat)
    {
        int axisIndex = Mathf.Abs(mapping);
        float value = 0f;
        switch (axisIndex)
        {
            case 1: value = quat.x; break;
            case 2: value = quat.y; break;
            case 3: value = quat.z; break;
            default: Debug.LogWarning("Invalid axis mapping: " + mapping); break;
        }
        return mapping < 0 ? -value : value;
    }

    void OnDataReceived(string message)
    {
        try
        {
            string[] values = message.Split(',');
            if (values.Length == 5)
            {
                int receivedPort = int.Parse(values[0]);
                if (receivedPort == targetPort)
                {
                    float w_in = float.Parse(values[1]);
                    float x_in = float.Parse(values[2]);
                    float y_in = float.Parse(values[3]);
                    float z_in = float.Parse(values[4]);

                    float sqrMagnitude = (w_in * w_in) + (x_in * x_in) + (y_in * y_in) + (z_in * z_in);

                    if (sqrMagnitude < 0.9f || sqrMagnitude > 1.1f)
                    {
                        // 異常値なので処理を中断（前の値を維持する）
                        Debug.LogWarning($"[Port {targetPort}] 異常データを破棄: sqrMag={sqrMagnitude}");
                        return;
                    }

                    float x_out = GetMappedAxis(quatX, new Quaternion(x_in, y_in, z_in, w_in));
                    float y_out = GetMappedAxis(quatY, new Quaternion(x_in, y_in, z_in, w_in));
                    float z_out = GetMappedAxis(quatZ, new Quaternion(x_in, y_in, z_in, w_in));
                    float w_out = w_in;

                    Quaternion newArduinoQuat = new Quaternion(x_out, y_out, z_out, w_out);

                    if (!isFirstDataReceived)
                    {
                        initialSensorRotation = newArduinoQuat;
                        receivedQuaternion = newArduinoQuat;
                        isFirstDataReceived = true;
                    }
                    else
                    {
                        float angleDifference = Quaternion.Angle(receivedQuaternion, newArduinoQuat);
                        if (angleDifference <= thresholdAngle)
                        {
                            receivedQuaternion = newArduinoQuat;
                        }
                        else
                        {
                            Debug.LogWarning($"[Port {targetPort}] スパイク検出。角度差: {angleDifference}度。");
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }
}