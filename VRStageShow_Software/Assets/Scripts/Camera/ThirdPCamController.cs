//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class CamController : MonoBehaviour
//{
//    public GameObject charaPos; //キャラクターのゲームオブジェクト
//    private Vector3 offset; //相対距離取得用

//    void Start()
//    {
//        offset = transform.position - charaPos.transform.position;
//    }

//    void Update()
//    {
//        //
//        transform.position = new Vector3(charaPos.transform.position.x+ offset.x, charaPos.transform.position.y + offset.y -0.2f, charaPos.transform.position.z + offset.z);
//    }
//}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPCameraController : MonoBehaviour
{
    [SerializeField] GameObject player;        // 追従対象

    [Header("カメラの相対位置（プレイヤー座標系）")]
    [SerializeField] float distanceBack = 1f;  // プレイヤー背後距離
    [SerializeField] float height = 1f;  // カメラの高さ

    [Header("注視点オフセット")]
    [SerializeField] float lookHeight = 0.5f;// プレイヤー pivot から何 m 上を見るか

    [Header("追従スムーズ係数")]
    [SerializeField] float posSmooth = 0.15f;
    [SerializeField] float rotSmooth = 8f;

    Vector3 targetPos;

    void Update()
    {
        /* --- 1) 位置追従 ---------------------------------------------------- */
        Vector3 localOffset = new Vector3(0, height, -distanceBack);
        targetPos = player.transform.TransformPoint(localOffset);
        transform.position = Vector3.Lerp(transform.position, targetPos, posSmooth);

        /* --- 2) 回転追従（水平寄りに） ------------------------------------- */
        Vector3 lookTarget = player.transform.position + Vector3.up * lookHeight;

        Quaternion targetRot = Quaternion.LookRotation(lookTarget - transform.position,
                                                        Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation,
                                              targetRot,
                                              Time.deltaTime * rotSmooth);
    }
}
