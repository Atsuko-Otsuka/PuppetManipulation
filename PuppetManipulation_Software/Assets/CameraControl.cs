using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

//	public Transform target;	// ターゲットの参照

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
//		this.transform.Rotate(1, 1, 1);
		Vector3 v = this.transform.position;
//		GetComponent<Transform>().position = //target.position;
		if (Input.GetKey(KeyCode.LeftArrow)){
			v.x -= 0.01f;
//			GetComponent<Transform>().position += 0.01f ; //target.position;
		}
		if (Input.GetKey(KeyCode.RightArrow)){
			v.x += 0.01f;
		}
		if (Input.GetKey(KeyCode.UpArrow)){
			v.y += 0.01f;
		}
		if (Input.GetKey(KeyCode.DownArrow)){
			v.y -= 0.01f;
		}
		if (Input.GetKey(KeyCode.G)){
			v.z += 0.01f;
		}
		if (Input.GetKey(KeyCode.B)){
			v.z -= 0.01f;
		}
		this.transform.position = v;
		v.x = 0.0f ;
		v.y = 0.0f ;
		v.z = 0.0f ;

	}
}
