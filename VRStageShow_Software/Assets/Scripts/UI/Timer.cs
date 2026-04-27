using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField]
    public TMP_Text TimerText;
    public TMP_Text InstructText;
    public float limitTime = 30; // êßå¿éûä‘

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        limitTime -= Time.deltaTime;

        if (limitTime < 0)
        {
            limitTime = 0;
        }

        if (limitTime < 6 && limitTime > 1)
        {
            InstructText.text = "Last";
        }
        if (limitTime < 1)
        {
            InstructText.text = "Pose";
        }
        
        TimerText.text = limitTime.ToString("F0"); // écÇËéûä‘ÇêÆêîÇ≈ï\é¶
    }
}