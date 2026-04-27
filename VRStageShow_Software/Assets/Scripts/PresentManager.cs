using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ここから持ってきた。
// http://ftvoid.com/blog/post/666
public class PresentManager: MonoBehaviour
{
    public GameObject presentL;
    public GameObject presentR;
    public GameObject apple;

    private bool isRunning = false; //コルーチン実行中判定
    private int a = -1;

	// Update is called once per frame
	void Start()
	{
        StartCoroutine(PresentFade(presentL, presentR, apple));
	}

    //presentのフェード
    IEnumerator PresentFade(GameObject prsL, GameObject prsR, GameObject apl)
    {
        while (true)
        {
            Color colorL = prsL.GetComponent<Renderer>().material.color;
            Color colorR = prsR.GetComponent<Renderer>().material.color;
            for (int i = 0; i < 20; i++)
            {
                colorL.a += 0.05f * a;
                colorR.a += 0.05f * a;

                prsL.GetComponent<Renderer>().material.color = colorL;
                prsR.GetComponent<Renderer>().material.color = colorR;

                yield return new WaitForSeconds(0.04f);
            }

            //フェードインした後appleの位置を入れ替える
            if (a > 0)
            {
                Vector3 pos = apl.transform.position;
                apple.transform.position = new Vector3(-pos.x, pos.y, pos.z);
            }
            a *= -1;

            yield return new WaitForSeconds(0.5f);
        }
 
    }
}