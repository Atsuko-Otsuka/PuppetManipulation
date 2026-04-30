using UnityEngine;
using System.Collections;


public class Santa : MonoBehaviour {

	private Animator animator;
	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.Space)) {
			animator.SetBool ("isRunning", true);
		}
/*		else if (Random.Range (0, 100) == 3) {
//			animator.SetBool ("isTest", true );
			animator.SetBool ("isRunning", true );
		}
		*/
		else if (Random.Range (0, 100) == 5) {
			animator.SetTrigger ("isTalk");
		}
		else if (Random.Range (0, 500) == 3) {
//			animator.SetTrigger ("isJump");
		}
		else if (Random.Range (0, 500) == 5) {
			animator.SetTrigger ("isWalk");
		}
		else if (Random.Range (0, 500) == 11) {
			animator.SetTrigger ("isHaHa");
		}
		else if (Random.Range (0, 500) == 13) {
			animator.SetTrigger ("isTest");
		}
		else if ( Input.GetKey(KeyCode.J) ) {
			animator.SetTrigger ("isJump");
		}
		else if ( Input.GetKey(KeyCode.A) ) {
			animator.SetBool ("isTest", true );
		}
		else 
		{
			animator.SetBool ("isRunning", false);
			animator.SetBool ("isTest", false);
//			animator.SetBool ("isTalk", false );
		}	
	}
}
