using UnityEngine;
using System.Collections;

public class LableBoard : MonoBehaviour {

	// Use this for initialization
	public Camera cameraToLookAt;

	public float far;
	// NpcName = null;
	public  Vector3 Cameraposition;
	public Vector3 Nameposition;
	void Start () {
	
		cameraToLookAt = Camera.main;

		//Nameposition =this.transform.position;

	} 


	void RotationEchange(Vector3 Camerapos,Vector3 Namepos)
	{
		Vector3 v =Camerapos - Namepos;
			far = v.magnitude;
		//far =Mathf.Abs(cmagn-NameP);


	//	if(far <=10.0f)
		//{
	//		this.transform.localScale =new Vector3 (0.01f,0.01f,1);
	//	}else if((far >=10.0f) &&(far <=20.0f))
	//	{
	//		float far0 =1-far*0.04f;
	//		this.transform.localScale =new Vector3 (0.012f*far0,0.012f*far0,1);
	//	}


	}

	void changeScale(float Nowfar)
	{

		//int rank = (int)Nowfar/4;
		//if(rank >1)
			//this.transform.localScale

	}

	// Update is called once per frame
	void Update () {
	
		Nameposition =this.transform.position;
		Cameraposition = cameraToLookAt.transform.position;
		RotationEchange(Cameraposition,Nameposition);
	}
}
