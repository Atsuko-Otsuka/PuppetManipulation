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
        Vector3 bone1Rot = bone1.localRotation.eulerAngles;
        Vector3 bone2Rot = bone2.localRotation.eulerAngles;

        // 腕を上げる
        if (bone1Rot.x > 5 && bone1Rot.x < 30 && bone1Rot.y > 5 && bone1Rot.y < 30 && bone1Rot.z > 5 && bone1Rot.z < 30) // 指定範囲
        {
            text.text = "Pose Detected: Thunder";

            if (!isThunderActive)
            {
                SpawnThunder();
                isThunderActive = true;
            }
        }
        else if (bone1Rot.x > 45 && bone1Rot.x < 70 && bone2Rot.x > 300 && bone2Rot.x < 330)
        {
            text.text = "Pose Detected: Power Up";

            if (!isPowerActive)
            {
                SpawnPowerUp();
                isPowerActive = true;
            }
        }
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
        Vector3 dir = rot * player.forward;
        Vector3 pos = player.position + dir * radius;
        Instantiate(thunderPrefab, pos, Quaternion.identity);
    }

    void SpawnPowerUp()
    {
        Instantiate(powerPrefab, player.position, Quaternion.identity);
    }
}
