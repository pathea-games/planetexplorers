using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;

public class BoundBoxScale : MonoBehaviour{
	public GameObject xyfar;
	public GameObject xynear;
	public GameObject yzfar;
	public GameObject yznear;
	public BoundBoxScale()
	{
	}
	public void SetRestrictionRange(int maxX, int maxY, int maxZ)
	{
		float w = (float)maxX;
		float h = (float)maxY;
		
		xyfar.transform.localScale = new Vector3((w - 2)/ 10.0f, 0, h / 10.0f);
		xynear.transform.localScale = new Vector3((w - 2) / 10.0f, 0, h / 10.0f);
		yzfar.transform.localScale = new Vector3((w - 2) / 10.0f, 0, h / 10.0f);
		yznear.transform.localScale = new Vector3((w - 2) / 10.0f, 0, h / 10.0f);
		
		
		xyfar.transform.localPosition = new Vector3(w / 2, h / 2, w - 1);
		
		xynear.transform.localPosition = new Vector3(w / 2, h / 2, 0.5f);
		
		yzfar.transform.localPosition = new Vector3(w - 1, h / 2, w / 2);
		
		yznear.transform.localPosition = new Vector3(0.5f, h / 2, w / 2);
	}
	


}
