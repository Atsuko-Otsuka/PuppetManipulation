using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;

// クリック処理の参考元
// https://qiita.com/Eureka/items/1f1e5330685fadb13d1b

public class QuestionManager: MonoBehaviour
{
    public GameObject apple;
    public GameObject presentL;
    public GameObject presentR;
    public GameObject bird;
    public GameObject textObject;
    public Camera camera_object; //カメラを取得

    private RaycastHit hit; //レイキャストが当たったものを取得する入れ物
    private AudioSource[] sounds;
    private Animator animator;
    private int count = 0;
    private Text text;
    private int f = 1; //1: 回答前，0: コルーチン実行中，-1: 回答後

    private void Start()
    {
        sounds = GetComponents<AudioSource>();
        animator = bird.GetComponent<Animator>();
        text = textObject.GetComponent<Text>();

     
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.anyKeyDown)
        {
            
            foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(code))
                {
                    string codeStr = code.ToString();

                    if (f > 0 && (code == KeyCode.Alpha1 || code == KeyCode.Alpha2))
                    {
                        Debug.Log(codeStr);
                        float objectPosition_x = codeStr == "Alpha1" ? presentL.transform.position.x : presentR.transform.position.x;

                        f = 0;
                        if (count == 1 || count == 5)
                        {
                            StartCoroutine(Answer(1, apple, objectPosition_x, presentL, presentR, sounds[2], "isCorrect", ""));
                            count = count < 5 ? count + 1 : 0;
                        }
                        else if (count < 5)
                        {
                            StartCoroutine(Answer(-1, apple, objectPosition_x, presentL, presentR, sounds[1], "isWrong", ""));
                            count += 1;
                        }
                    }
                    else if (f < 0 && code == KeyCode.Alpha3)
                    {
                        f = 0;

                        //回答後再度クリックするとシンキングタイムに戻る
                        StartCoroutine(Thinking(apple, presentL, presentR));
                        //シンキングアニメーション&BGM&テキスト
                        animator.SetTrigger("isIdle");
                        sounds[0].Play();
                        text.text = "";
                    }
                }
            }
        }
    }

    //appleを配置
    //presentをフェードアウトさせて正誤表示（正解: a=1，不正解: a=-1）
    IEnumerator Answer(int a, GameObject apl, float x, GameObject prsL, GameObject prsR, AudioSource sound, string animTrigger, string ansText)
    { 
        //appleを配置（正解ならクリックしたpresentの位置，不正解なら反対側）
        Vector3 pos = apl.transform.position;
        apple.transform.position = new Vector3(a*x, pos.y, pos.z);

        //presentのフェードアウト
        Color colorL = prsL.GetComponent<Renderer>().material.color;
        Color colorR = prsR.GetComponent<Renderer>().material.color;
        for (int i=0; i<20; i++)
        {
            colorL.a -= 0.05f;
            colorR.a -= 0.05f;

            prsL.GetComponent<Renderer>().material.color = colorL;
            prsR.GetComponent<Renderer>().material.color = colorR;

            yield return new WaitForSeconds(0.04f);
        }
        prsL.SetActive(false);
        prsR.SetActive(false);

        //BGMを止める
        sounds[0].Stop();
        //音再生
        sound.PlayOneShot(sound.clip);
        //アニメーション
        animator.SetTrigger(animTrigger);
        //テキスト表示
        text.text = ansText;

        f = -1;
    }

    //appleをフェードアウトさせてpresentをフェードイン
    IEnumerator Thinking(GameObject apl, GameObject prsL, GameObject prsR)
    {
        //appleのフェードアウト
        GameObject apl_child = apl.transform.GetChild(0).gameObject;
        Color color = apl_child.GetComponent<Renderer>().material.color;
        Color colorL = prsL.GetComponent<Renderer>().material.color;
        Color colorR = prsR.GetComponent<Renderer>().material.color;
        for (int i = 0; i < 10; i++)
        {
            color.a -= 0.1f;
            apl_child.GetComponent<Renderer>().material.color = color;

            yield return new WaitForSeconds(0.04f);
        }

        //presentのフェードイン
        prsL.SetActive(true);
        prsR.SetActive(true);
        for (int i = 0; i < 10; i++)
        {
            colorL.a += 0.1f;
            colorR.a += 0.1f;

            prsL.GetComponent<Renderer>().material.color = colorL;
            prsR.GetComponent<Renderer>().material.color = colorR;

            yield return new WaitForSeconds(0.05f);
        }

        //appleの透明度を戻す
        color.a = 1.0f; 
        apl_child.GetComponent<Renderer>().material.color = color;

        f = 1;
    }
}
