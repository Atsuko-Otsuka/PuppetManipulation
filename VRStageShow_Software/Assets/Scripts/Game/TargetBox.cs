using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBox : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cube"))
        {
            Debug.Log("Success " + other.name);

            other.transform.SetParent(null); // 親解除（重要）
            Destroy(other.gameObject);
        }
    }
}
