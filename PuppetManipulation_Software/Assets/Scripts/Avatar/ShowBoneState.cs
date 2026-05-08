using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowBoneState : MonoBehaviour
{
    public TMP_Text text;
    Animator animator;
    public HumanBodyBones targetBone;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // 各ボーンのTransform取得
        Transform bone = animator.GetBoneTransform(targetBone);

        // オイラー角に変換
        Vector3 boneRot = bone.rotation.eulerAngles;
        
        text.text = "BoneState:" + boneRot;
    }
}
