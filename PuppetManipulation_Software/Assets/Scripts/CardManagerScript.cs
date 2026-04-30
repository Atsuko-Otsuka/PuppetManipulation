using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CardManagerScript : MonoBehaviour
{
    [SerializeField]
    private GameObject cardLeft;
    [SerializeField]
    private GameObject cardRight;
    [SerializeField]
    private GameObject cursor;

    private Material ansMat;
    private Material failMat;

    private void Start()
    {
        ansMat = Resources.Load("Materials/Card_ans") as Material;
        failMat = Resources.Load("Materials/Card_fail") as Material;
        cursor.SetActive(false);
    }

    void Update()
    {
        // a/b/c/d/を入力したらカードを選択し，カードのマークを変更する．
        if(Input.GetKeyDown("a"))
        {
            cardRight.GetComponent<Renderer>().material = ansMat;
            cardLeft.GetComponent<Renderer>().material = failMat;
  //          selectRight();
        } 
        else if(Input.GetKeyDown("b"))
        {
   //         cardLeft.GetComponent<Renderer>().material = ansMat;
   //         cardRight.GetComponent<Renderer>().material = failMat;
  //          selectLeft();
            cardRight.GetComponent<Renderer>().material = failMat;
            cardLeft.GetComponent<Renderer>().material = ansMat;
        }
        else if(Input.GetKeyDown("c"))
        {
 //           cardRight.GetComponent<Renderer>().material = failMat;
 //           cardLeft.GetComponent<Renderer>().material = ansMat;
            selectRight();
        }
        else if(Input.GetKeyDown("d"))
        {
 //           cardLeft.GetComponent<Renderer>().material = failMat;
  //          cardRight.GetComponent<Renderer>().material = ansMat;
           selectLeft();
        } 
        // Enterを押すと，カードが裏面になる
        else if(Input.GetKeyDown(KeyCode.Return))
        {
            CardClose();
            cursor.SetActive(false);

        }
        // Spaceを押すとやめないで！のシーン
/*        else if(Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("Answer");
        }
*/
        // oを押すと，カードを開く
        else if(Input.GetKeyDown("o"))
        {
            CardOpen();
        }
    }

    void CardOpen()
    {
        cardLeft.transform.rotation = Quaternion.Euler(90, 0, 180);
        cardRight.transform.rotation = Quaternion.Euler(90, 0, 180);
    }

    void CardClose()
    {
        cardLeft.transform.rotation = Quaternion.Euler(90, 0, 0);
        cardRight.transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    void selectRight()
    {
        cursor.SetActive(true);
        cursor.transform.position = cardRight.transform.position;
    }

    void selectLeft()
    {
        cursor.SetActive(true);
        cursor.transform.position = cardLeft.transform.position;
    }
}
