using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeckRotateTarget : MonoBehaviour
{

	public UDPReceiver updReceiver;
	public Transform ObjectTf;
	public Quaternion ObjectRot;
	public float calibYaw;

	// Use this for initialization
	void Start()
	{
		UDPReceiver.AccelCallBack += AccelAction;
		UDPReceiver.GyroCallBack += GyroAction;
		updReceiver.UDPStart();
	}

	public void AccelAction(float xx, float yy, float zz)
	{

	}

	public void GyroAction(float xx, float yy, float zz, float ww)
	{
		/*teddybear*/
		//var newQut = new Quaternion(yy, -xx, zz, ww);
		var newQut = new Quaternion(-xx, -zz, -yy, ww);
		var newRot = newQut * Quaternion.Euler(90f, 0, 0) * Quaternion.Euler(0f, 0f, 90f);
		//newRot = newRot * Quaternion.Euler(0f,0f , 90f);
		ObjectRot = newRot;
	}

	// Update is called once per frame
	void Update()
	{
		ObjectTf.localRotation = ObjectRot;
		var dtRot = 0.2f;

		if (Input.GetKey(KeyCode.LeftArrow))
		{
			calibYaw += dtRot;
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			calibYaw -= dtRot;
		}
		ObjectTf.Rotate(0f, calibYaw, 0, Space.World);
		ObjectTf.Rotate(20f, 0, 0, Space.Self);
	}
}
