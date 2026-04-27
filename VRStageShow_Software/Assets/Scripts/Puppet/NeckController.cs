using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//iPhoneの傾きを首の動きに対応させるプログラム（使っていない）
public class NeckController : MonoBehaviour
{

	public UDPReceiver updReceiver;
	public Transform neckTf;
	public Transform spineTf;
	public Quaternion neckRot;
	public Quaternion spineRot;
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
		var newQut = new Quaternion(zz, xx, yy, ww);
		var newRot = newQut * Quaternion.Euler(0, 0f, -90f);
		/*bananaman*/
		//var newQut = new Quaternion(yy, xx, zz, ww);
		//var newRot = newQut * Quaternion.Euler(-90, 0f, 0f);
		neckRot = newRot;
		var newSpineQut = new Quaternion(zz,xx, yy, ww);
		var newSpineRot = newSpineQut * Quaternion.Euler(0, 0f, -90f);
		spineRot = newSpineRot;
	}

	// Update is called once per frame
	void Update()
	{
		neckTf.localRotation = neckRot;
		neckTf.Rotate(0, calibYaw, 0, Space.World);
		spineTf.localRotation = spineRot;
		spineTf.Rotate(0, calibYaw, 0, Space.World);
	}
}
