using UnityEngine;
using System.Collections;

public class Outline : MonoBehaviour
{
    [SerializeField]
    bool enable = true;

    [SerializeField]
    Color color = Color.white;

    [SerializeField]
    SpriteRenderer[] outlines;

    void OnValidate()
    {
        SetOutlineActive();
        SetOutlineColor();
    }

    void SetOutlineActive()
    {
        if (outlines != null && outlines.Length > 0)
        {
            foreach (SpriteRenderer go in outlines)
            {
                if (go != null && go.gameObject != null) go.gameObject.SetActive(enable);
            }
        }
    }

    void SetOutlineColor()
    {
        if (outlines != null && outlines.Length > 0)
        {
            foreach (SpriteRenderer go in outlines)
            {
                if (go != null) go.color = color;
            }
        }
    }
}
