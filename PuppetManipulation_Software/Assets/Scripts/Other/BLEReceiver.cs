using UnityEngine;
using System;

public class BLEReceiver : MonoBehaviour
{
    // マイコン側で設定した UUID と一致させる
    public string serviceUUID = "9d8c03f6-a988-4647-9474-dedaedf309e0";
    public string characteristicUUID = "bfbadb76-f1d7-4b84-9e66-82ac847e21fa";

    // アバターの各部位の Transform をアサインする（配列で管理）
    public Transform[] avatarParts;

    // BLEプラグインからのデータ受信イベントなどで呼ばれる関数
    public void OnDataReceived(string rawData)
    {
        Debug.LogWarning("Start BLE Recever");
        // rawData の例: "0,1.000,0.000,0.000,0.000;"
        try
        {
            // セミコロンで複数のセンサーデータを分割
            string[] packages = rawData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string package in packages)
            {
                Debug.LogWarning("Get Data via BLE");
                // カンマで ID, W, X, Y, Z を分割
                string[] values = package.Split(',');
                if (values.Length < 5) continue;

                int sensorId = int.Parse(values[0]);
                float w = float.Parse(values[1]);
                float x = float.Parse(values[2]);
                float y = float.Parse(values[3]);
                float z = float.Parse(values[4]);

                Debug.LogWarning("Split Data via BLE");

                // BNO055(W,X,Y,Z) を Unity(X,Y,Z,W) に変換
                // ※軸の入れ替えが必要な場合があります
                Quaternion sensorRotation = new Quaternion(x, y, z, w);

                // 指定した ID の部位に回転を適用
                if (sensorId < avatarParts.Length && avatarParts[sensorId] != null)
                {
                    // 親要素の回転を考慮する場合は localRotation
                    avatarParts[sensorId].localRotation = sensorRotation;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Data parse error: " + e.Message);
        }
    }
}