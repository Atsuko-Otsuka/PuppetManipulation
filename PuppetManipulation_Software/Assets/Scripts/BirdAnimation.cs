using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdAnimation : MonoBehaviour
{
    private Animator animator;
    private AudioSource[] sounds;

    // Start is called before the first frame update
    void Start()
    {
        sounds = GetComponents<AudioSource>();
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            animator.SetTrigger("isY");
            sounds[0].Play();
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            animator.SetTrigger("isH");
            sounds[1].Play();
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            animator.SetTrigger("isU");
            sounds[2].Play();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            animator.SetTrigger("isJ");
            sounds[3].Play();
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            animator.SetTrigger("isZ");
            sounds[4].Play();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            animator.SetTrigger("isX");
            sounds[5].Play();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            animator.SetTrigger("isC");
            sounds[6].Play();
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            animator.SetTrigger("isV");
            sounds[7].Play();
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            animator.SetTrigger("isB");
            sounds[8].Play();
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            animator.SetTrigger("isN");
            sounds[9].Play();
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            animator.SetTrigger("isM");
            sounds[10].Play();
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            animator.SetTrigger("isG");
            sounds[11].Play();
        }
    }
}
