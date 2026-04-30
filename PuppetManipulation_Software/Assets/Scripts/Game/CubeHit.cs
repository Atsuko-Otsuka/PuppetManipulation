using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CubeHit : MonoBehaviour
{
    private int hitCount = 0;
    public TMP_Text hitCountText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "CollectCube")
        {
            Debug.Log("Hit "+ collision.gameObject.name);
            Destroy(collision.gameObject);
            hitCount++;
            hitCountText.text = "Count: " + hitCount.ToString();
        }
    }
}
