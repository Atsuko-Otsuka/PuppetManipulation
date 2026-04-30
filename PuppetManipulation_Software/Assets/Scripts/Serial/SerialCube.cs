using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SerialCube : MonoBehaviour
{

    public SerialHandler serialHandler;
    public TMP_Text text;
    public GameObject cube;

    // Use this for initialization
    void Start()
    {
        // 信号を受信したときに、そのメッセージを記録
        serialHandler.OnDataReceived += OnDataReceived;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*
	 * シリアルを受けたときの処理
	 */
    void OnDataReceived(string message)
    {
        try
        {
            string[] angles = message.Split(',');
            text.text = "ax:" + angles[0] + " " + "ay:" + angles[1] + " " + "az:" + angles[2] + "\n" + "gx:" + angles[3] + " " + "gy:" + angles[4] + " " + "gz:" + angles[5] + "\n" + "mx:" + angles[6] + " " + "my:" + angles[7] + " " + "mz:" + angles[8] + "\n"; // �V���A���̒l���e�L�X�g�ɕ\��


            //Vector3 angle = new Vector3(float.Parse(angles[0]), float.Parse(angles[2]), float.Parse(angles[1]));
            //Vector3 angle = new Vector3(float.Parse(angles[2]), float.Parse(angles[3]), float.Parse(angles[4]));
            //Vector3 angle = new Vector3(float.Parse(angles[5]), float.Parse(angles[6]), float.Parse(angles[7]));
            //cube.transform.rotation = Quaternion.Euler(angle);

//            float yaw_z = float.Parse(values[0]);
//            float pitch_y = float.Parse(values[1]);
//            float roll_x = float.Parse(values[2]);

//            // 1. Z軸 (Yaw) の回転
//            Quaternion zRot = Quaternion.AngleAxis(yawZ, Vector3.forward);

//            // 2. Y軸 (Pitch) の回転
//            Quaternion yRot = Quaternion.AngleAxis(pitchY, Vector3.up);

//            // 3. X軸 (Roll) の回転
//            Quaternion xRot = Quaternion.AngleAxis(rollX, Vector3.right);

//            // 回転の合成順序: Z -> Y -> X
//            // (Unityの回転合成は、Q_final = Qz * Qy * Qx と記述する)
//            cube.transform.rotation = zRot * yRot * xRot;
//
//            Vector3 angle = new Vector3(float.Parse(angles[5]), float.Parse(angles[6]), float.Parse(angles[7]));
//            cube.transform.rotation = Quaternion.Euler(angle);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }
}