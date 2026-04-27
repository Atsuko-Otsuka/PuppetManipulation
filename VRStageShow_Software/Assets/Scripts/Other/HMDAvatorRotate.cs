using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HMDAvatorRotate : MonoBehaviour
{
    public GameObject targetObject;    // 回転させたいCubeやボーンを指定
    public GameObject hmdObject;    // hmdの回転をマッピングしたオブジェクト
    public GameObject bodyObject; // 体の向きの基準となるオブジェクト

    //[Header("Quaternion軸マッピング (1=x, 2=y, 3=z, 負の数で反転)")]
    //// センサーとUnityの軸が違う場合ここで調整します
    //public int quatX = 1;
    //public int quatY = 2;
    //public int quatZ = 3;

    [Header("動きの滑らかさ")]
    // 1.0に近いほどキビキビ、0.1に近いほどフワッと動く
    [Range(0.01f, 1.0f)]
    public float smoothSpeed = 0.2f;

    void Start()
    {
        
    }

    void LateUpdate()
    {
        // HMDの回転を取得
        Quaternion hmdRotation = hmdObject.transform.rotation;
        // アバターの体の向き(Yaw軸)を取得する
        float bodyYawAngle = bodyObject.transform.eulerAngles.y;
        Quaternion bodyYawRotation = Quaternion.Euler(0, bodyYawAngle, 0);

        // 回転の合成：「体の向き」に対して「センサーの動き」を加える
        Quaternion targetRotation = bodyYawRotation * hmdRotation;

        // 差分回転をオイラー角に変換（角度制限や正規化のため）
        Vector3 relativeEuler = targetRotation.eulerAngles;

        // オイラー角を -180度～180度の範囲に正規化
        float pitch = NormalizeAngle(relativeEuler.x);
        float yaw = NormalizeAngle(relativeEuler.y);
        float roll = NormalizeAngle(relativeEuler.z);

        // 正規化した角度から、ターゲットに向けた回転を作成
        Quaternion clampedRotation = Quaternion.Euler(pitch, yaw, roll);

        // ターゲットの「ローカル回転」に適用
        // Slerpを使って滑らかに回転させる
        targetObject.transform.localRotation = Quaternion.Slerp(
            targetObject.transform.localRotation, // 現在の回転
            clampedRotation,                      // 目標の回転
            smoothSpeed                           // 補間スピード
        );
    }

    // 角度を -180 ～ 180 度に変換する関数
    private float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        if (angle > 180) angle -= 360;
        else if (angle < -180) angle += 360;
        return angle;
    }

    //// 軸のマッピング処理
    //private float GetMappedAxis(int mapping, Quaternion quat)
    //{
    //    int axisIndex = Mathf.Abs(mapping);
    //    float value = 0f;
    //    switch (axisIndex)
    //    {
    //        case 1: value = quat.x; break;
    //        case 2: value = quat.y; break;
    //        case 3: value = quat.z; break;
    //        default: Debug.LogWarning("Invalid axis mapping: " + mapping); break;
    //    }
    //    // 負の数なら反転
    //    return mapping < 0 ? -value : value;
    //}

    //// データ受信時の処理
    //void OnDataReceived(string message)
    //{
    //    try
    //    {
    //        // Arduinoからのデータ形式例: "0, 0.9, 0.1, 0.0, 0.0" (Port, w, x, y, z)
    //        string[] values = message.Split(','); // カンマ区切りで分割

    //        // データ長が足りているか確認
    //        if (values.Length >= 5)
    //        {
    //            int receivedPort = int.Parse(values[0]); // ポート番号確認
    //            if (receivedPort == targetPort)
    //            {
    //                // 文字列を数値に変換 (w, x, y, z の順と仮定)
    //                float w_in = float.Parse(values[1]);
    //                float x_in = float.Parse(values[2]);
    //                float y_in = float.Parse(values[3]);
    //                float z_in = float.Parse(values[4]);

    //                // データの健全性チェック (正規化されているか)
    //                float sqrMagnitude = (w_in * w_in) + (x_in * x_in) + (y_in * y_in) + (z_in * z_in);
    //                if (sqrMagnitude < 0.9f || sqrMagnitude > 1.1f)
    //                {
    //                    // 異常値の場合は無視
    //                    return;
    //                }

    //                // 軸のマッピングを適用して新しいQuaternionを作成
    //                // 元データの一時的なQuaternion
    //                Quaternion rawQuat = new Quaternion(x_in, y_in, z_in, w_in);

    //                float x_out = GetMappedAxis(quatX, rawQuat);
    //                float y_out = GetMappedAxis(quatY, rawQuat);
    //                float z_out = GetMappedAxis(quatZ, rawQuat);
    //                // w成分は通常そのまま使用するか、軸変換に伴い符号調整が必要な場合もありますが、
    //                // ここでは入力のwをそのまま使います
    //                float w_out = w_in;

    //                Quaternion newArduinoQuat = new Quaternion(x_out, y_out, z_out, w_out);

    //                // 初回受信時の処理（リセット/キャリブレーション）
    //                if (!isFirstDataReceived)
    //                {
    //                    initialSensorRotation = newArduinoQuat;
    //                    receivedQuaternion = newArduinoQuat;
    //                    isFirstDataReceived = true;
    //                    Debug.Log("Initial Sensor Rotation Set: キャリブレーション完了");
    //                }
    //                else
    //                {
    //                    // 2回目以降は変数を更新するだけ
    //                    receivedQuaternion = newArduinoQuat;
    //                }
    //            }
    //        }
    //    }
    //    catch (System.Exception e)
    //    {
    //        // パースエラーなどが起きても止まらないように警告だけ出す
    //        Debug.LogWarning("Data Parse Error: " + e.Message);
    //    }
    //}
}