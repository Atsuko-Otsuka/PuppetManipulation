using UnityEngine;
using System.Collections;

public class PlayingInstrument : MonoBehaviour
{
    public AudioClip clip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            StartCoroutine(PlayForOneSecond());
        }
    }

    IEnumerator PlayForOneSecond()
    {
        audioSource.PlayOneShot(clip);
        Debug.Log("Playing " + clip.name);
        yield return new WaitForSeconds(1f); // 1ïbë“Ç¬
        audioSource.Stop(); // ã≠êßí‚é~
    }
}