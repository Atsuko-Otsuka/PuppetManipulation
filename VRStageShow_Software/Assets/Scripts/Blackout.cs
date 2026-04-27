using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Blackout : MonoBehaviour
{
    private Image image;

    private void Start()
    {
        image = this.GetComponent<Image>();
        image.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            image.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        }
    }
}
