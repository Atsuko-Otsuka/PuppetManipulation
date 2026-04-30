using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingMove : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Animator anim;
    private const string WalkSpeed = "WalkSpeed";
    private Vector3 prePos; //1フレーム前の位置
    private Vector3 velocity; //速度
    public float WalkMotionSpeedScale = 2.0f; //歩くアニメーションの速度を調整するパラメータ
    private int animCount = 0; // アニメーションを再生した回数
    public bool doShaking;

    void Start()
    {
        prePos = transform.position; //初期位置を保持
        //WalkMotionSpeedScale = 6.0f; //歩くアニメーションの速度のデフォルト値
    }

    void CalcVelocityExample()
    {
        // deltaTimeが0の場合は何もしない
        if (Mathf.Approximately(Time.deltaTime, 0))
            return;

        // 現在位置取得
        var position = transform.position;

        // 現在速度計算
        velocity = (position - prePos) / Time.deltaTime;

        // 現在速度をログ出力
        //print($"velocity = {velocity}, position = {position}, prePos = {prePos}");

        // 前フレーム位置を更新
        prePos = position;
    }

    void WalkAnimation()
    {
        //前進
        if (Mathf.Abs(velocity.x) > 0 || Mathf.Abs(velocity.z) > 0)
        {
            float walkSpeed = Mathf.Abs(velocity.magnitude) * WalkMotionSpeedScale;
            anim.SetFloat(WalkSpeed, walkSpeed);
            anim.SetBool("Forward", true);
            //print($"walkspeed = {walkSpeed}");
        }
        else
        {
            if(doShaking)
            {
                animCount++;
                if(animCount > 200)
                {
                    anim.SetFloat(WalkSpeed, 0);
                    animCount = 0;
                }                
            }
            anim.SetBool("Forward", false);
            //Debug.Log(anim.GetFloat("WalkSpeed"));
        }

        ////後進
        //if (velocity.z < 0)
        //{
        //    float walkSpeed = velocity.magnitude * WalkMotionSpeedScale;
        //    anim.SetFloat(WalkSpeed, walkSpeed);
        //    anim.SetBool("Backward", true);
        //}
        //else
        //{
        //    anim.SetBool("Backward", false);
        //}
        
    }

    // Update is called once per frame
    void Update()
    {
        //移動速度を計算
        CalcVelocityExample();

        //歩くアニメーションを駆動
        WalkAnimation();
    }

   
}
