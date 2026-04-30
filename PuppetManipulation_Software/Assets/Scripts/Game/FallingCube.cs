using UnityEngine;

public class FallingCube : MonoBehaviour
{
    private bool isCaught = false;

    void OnTriggerEnter(Collider other)
    {
        if (isCaught) return;

        if (other.CompareTag("Hand"))
        {
            Catch(other.transform);
        }
    }

    void Catch(Transform hand)
    {
        isCaught = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        transform.SetParent(hand);
        transform.localPosition = new Vector3(-0.1f, 0.15f, 0);

        Debug.Log("Auto Caught");
    }
}