using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuatRotator : MonoBehaviour
{
    public SerialHandler serialHandler;
    public TMP_Text text;
    public GameObject targetObject;

    public int targetPort = 0;

    // Start is called before the first frame update
    void Start()
    {
        //信号を受信したときに、そのメッセージの処理を行う
        serialHandler.OnDataReceived += OnDataReceived;
    }

    // Update is called once per frame
    void Update()
    {
        
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
                    text.text = $"Port:{values[0]}, w:{values[1]}, x:{values[2]}, y:{values[3]}, z:{values[4]}\n";

                    float w = float.Parse(values[1]); // values[0] ではなく [1]
                    float x = float.Parse(values[2]); // values[1] ではなく [2]
                    float y = float.Parse(values[3]); // values[2] ではなく [3]
                    float z = float.Parse(values[4]); // values[3] ではなく [4]

                    // UnityのQuaternionは (x, z, y, w) の順でコンストラクタに渡す
                    // (BNO055の座標系からUnityの座標系への変換)
                    Quaternion arduinoQuat = new Quaternion(x, z, y, w);

                    // オブジェクトの回転に適用
                    targetObject.transform.rotation = arduinoQuat;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }
}
