using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JointLocalRotation : MonoBehaviour
{
    public SerialHandler serialHandler;
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

    //// 左右の回転 (Y軸)
    //[SerializeField] private float maxYaw = 80.0f;   // 右方向
    //[SerializeField] private float minYaw = -80.0f;  // 左方向

    //// 上下の回転 (X軸)
    //[SerializeField] private float maxPitch = 60.0f; // 上方向
    //[SerializeField] private float minPitch = -60.0f; // 下方向

    //// 傾き (Z軸)
    //[SerializeField] private float maxRoll = 40.0f;  // 右傾き
    //[SerializeField] private float minRoll = -40.0f; // 左傾き

    // ★ 1. Inspectorで監視したいポート番号を指定できるようにする
    public int targetPort = 0;

    // Quarternionに渡す回転軸の順番を指定する
    [Header("Quaternion軸マッピング (1=x, 2=y, 3=z, 負の数で反転)")]
    public int quatX = 1;
    public int quatY = 2;
    public int quatZ = 3;

    [Header("スパイク除去")]
    // 1フレームで変化を許容する最大角度（これを超えたら無視する）
    [Range(10.0f, 360.0f)]
    public float thresholdAngle = 360.0f;

    [Header("Smoothing")]
    [Range(1f, 20f)]
    public float smoothingFactor = 10.0f;

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
        //targetObject.transform.localRotation = clampedRotation;
        targetObject.transform.localRotation = Quaternion.Slerp(
        targetObject.transform.localRotation, // 現在の実際の回転
        clampedRotation,                      // センサーが示す目標の回転
        Time.deltaTime * smoothingFactor      // この値で滑らかに追従する
        );
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

    // Quarternionに渡す軸の順番を指定するヘルパー関数
    private float GetMappedAxis(int mapping, Quaternion quat)
    {
        int axisIndex = Mathf.Abs(mapping);
        float value = 0f;
        switch (axisIndex)
        {
            case 1:
                value = quat.x;
                break;
            case 2:
                value = quat.y;
                break;
            case 3:
                value = quat.z;
                break;
            default:
                Debug.LogWarning("Invalid axis mapping: " + mapping);
                break;
        }
        return mapping < 0 ? -value : value;
    }

    void OnDataReceived(string message)
    {
        try
        {
            // "ポート番号,w,x,y,z" の形式で分割
            string[] values = message.Split(',');

            // ★ 2. データが5個（ポート番号 + 4つのクォータニオン値）あるか確認
            if (values.Length == 5)
            {
                // ★ 3. 最初の要素をポート番号としてパース
                int receivedPort = int.Parse(values[0]);
                // ★ 4. 受信したポート番号が、指定したポート番号と一致するか確認
                if (receivedPort == targetPort)
                {
                    // --- ポート番号が一致した場合のみ、以下の処理を実行 ---

                    // ★ 5. インデックスが1つずれる
                    //text.text = $"Port:{values[0]}, w:{values[1]}, x:{values[2]}, y:{values[3]}, z:{values[4]}\n";

                    float w_in = float.Parse(values[1]);
                    float x_in = float.Parse(values[2]);
                    float y_in = float.Parse(values[3]);
                    float z_in = float.Parse(values[4]);

                    float x_out = GetMappedAxis(quatX, new Quaternion(x_in, y_in, z_in, w_in));
                    float y_out = GetMappedAxis(quatY, new Quaternion(x_in, y_in, z_in, w_in));
                    float z_out = GetMappedAxis(quatZ, new Quaternion(x_in, y_in, z_in, w_in));
                    float w_out = w_in;

                    // 新しく受信した回転データ
                    Quaternion newArduinoQuat = new Quaternion(x_out, y_out, z_out, w_out);

                    // 最初のデータを受信したとき
                    if (!isFirstDataReceived)
                    {
                        initialSensorRotation = newArduinoQuat;
                        receivedQuaternion = newArduinoQuat; // 最初の値を「前回の値」として保存
                        isFirstDataReceived = true;
                    }
                    // 2回目以降のデータ（スパイク除去チェック）
                    else
                    {
                        // 1フレーム前の回転(receivedQuaternion)と新しい回転(newArduinoQuat)の間の角度（度）を計算
                        float angleDifference = Quaternion.Angle(receivedQuaternion, newArduinoQuat);

                        // 角度差が、設定したしきい値（thresholdAngle）以下の場合のみ、値を採用
                        if (angleDifference <= thresholdAngle)
                        {
                            // 正常な値として、回転を更新
                            receivedQuaternion = newArduinoQuat;
                        }
                        else
                        {
                            // 異常値（スパイク）と判断。
                            // receivedQuaternion を更新しないことで、前回の値をそのまま維持する。
                            Debug.LogWarning($"[Port {targetPort}] スパイク検出。角度差: {angleDifference}度。データを無視します。");
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