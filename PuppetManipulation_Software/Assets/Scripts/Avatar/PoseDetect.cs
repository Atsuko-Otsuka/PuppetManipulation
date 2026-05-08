using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PoseDetect : MonoBehaviour
{
    public TMP_Text text;
    Animator animator;
    public HumanBodyBones targetBone1;
    public HumanBodyBones targetBone2;
    public Transform player;
    public GameObject thunderPrefab;
    public GameObject powerPrefab;

    private bool isThunderActive = false;
    private bool isPowerActive = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // 指定ボーンのTransform取得
        Transform bone1 = animator.GetBoneTransform(targetBone1);
        Transform bone2 = animator.GetBoneTransform(targetBone2);

        // オイラー角に変換
        //Vector3 bone1Rot = bone1.localRotation.eulerAngles;
        //Vector3 bone2Rot = bone2.localRotation.eulerAngles;

        // ボーンの指し示す方向
        Vector3 rightArmDir = -bone1.right;
        // 比較対象
        Vector3 targetDir1 = player.up;
        // 内積計算
        float dot1 = Vector3.Dot(rightArmDir, targetDir1);
        Debug.Log("Dot Product: " + dot1);
        //矢印を可視化
        ////青い線：ボーンが認識している「腕の向き」
        //Debug.DrawRay(bone1.position, rightArmDir * 1.0f, Color.blue);
        ////赤い線：判定の基準にしている「目標の向き」
        //Debug.DrawRay(bone1.position, targetDir1 * 1.0f, Color.red);

        // ボーンの指し示す方向
        Vector3 rightArmDir2 = -bone1.up;
        Vector3 leftArmDir = -bone2.up;
        // 比較対象
        Vector3 targetDir2 = player.right;
        // 内積計算
        float dot2 = Vector3.Dot(rightArmDir2, targetDir2);
        float dot3 = Vector3.Dot(leftArmDir, targetDir2);
        Debug.Log("Dot Product: " + dot2 + ", " + dot3);
        //矢印を可視化
        //青い線：ボーンが認識している「腕の向き」
        Debug.DrawRay(bone1.position, rightArmDir2 * 1.0f, Color.blue);
        //赤い線：判定の基準にしている「目標の向き」
        Debug.DrawRay(bone1.position, targetDir2 * 1.0f, Color.red);
        //青い線：ボーンが認識している「腕の向き」
        Debug.DrawRay(bone2.position, leftArmDir * 1.0f, Color.blue);
        //赤い線：判定の基準にしている「目標の向き」
        Debug.DrawRay(bone2.position, targetDir2 * 1.0f, Color.red);

        // 腕を上げる
        if (dot1 > 0.9f)
        {
            if (!isThunderActive)
            {
                SpawnThunder();
                isThunderActive = true;
            }

        }
        else if (dot2 < -0.9f && dot3 < -0.9f)
        {
            if (!isPowerActive)
            {
                SpawnPowerUp();
                isPowerActive = true;
            }
        }


        //if (bone1Rot.x > 5 && bone1Rot.x < 30 && bone1Rot.y > 5 && bone1Rot.y < 30 && bone1Rot.z > 5 && bone1Rot.z < 30) // 指定範囲
        //{
        //    text.text = "Pose Detected: Thunder";

        //    if (!isThunderActive)
        //    {
        //        SpawnThunder();
        //        isThunderActive = true;
        //    }
        //}
        //else if (bone1Rot.x > 45 && bone1Rot.x < 70 && bone2Rot.x > 300 && bone2Rot.x < 330)
        //{
        //    text.text = "Pose Detected: Power Up";

        //    if (!isPowerActive)
        //    {
        //        SpawnPowerUp();
        //        isPowerActive = true;
        //    }
        //}
        else
        {
            text.text = "Pose Not Detected";
            isThunderActive = false;
            isPowerActive = false;
        }
    }

    void SpawnThunder()
    {
        float radius = 3f;
        Quaternion rot = Quaternion.Euler(0, 0, 0);
        Vector3 dir = rot * player.up;
        Vector3 pos = player.position + dir * radius;
        Instantiate(thunderPrefab, pos, Quaternion.identity);
    }

    void SpawnPowerUp()
    {
        Instantiate(powerPrefab, player.position, Quaternion.identity);
    }
}
