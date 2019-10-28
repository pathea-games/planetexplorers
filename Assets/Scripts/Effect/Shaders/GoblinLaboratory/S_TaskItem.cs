using UnityEngine;
using System.Collections;

public class S_TaskItem : MonoBehaviour {
	MeshRenderer smr;
	public float angle = 0f;
	public float Xmin = -0.25f;
	public float Xmax = 0.25f;
	public float Zmin = -0.16475f;
	public float Zmax = 0.6494f;
	public float cycle = 3f;
	public float speed = 5f;
	public float wide = 0.25f;
	float ctime = 0f;
	float coolingTime;
	
	void Start () {
		smr = transform.GetComponent<MeshRenderer>();
		Material mat = Resources.Load("Materials/TaskItem") as Material;
		Texture tx = smr.GetComponent<Renderer>().material.GetTexture(0);
		mat.SetTexture(0, tx);
		smr.GetComponent<Renderer>().material = mat;
		mat.SetFloat("_Angle", Mathf.Clamp(angle / 180 * Mathf.PI, 0, 3.1416f));
		mat.SetFloat("_Xmin", Xmin);
		mat.SetFloat("_Xmax", Xmax);
		mat.SetFloat("_Zmin", Zmin);
		mat.SetFloat("_Zmax", Zmax);	
		mat.SetFloat("_Wide", wide);
		mat.SetFloat("_Speed", speed);
	}
	
	void Update () {
		coolingTime = (Mathf.Max(Xmax - Xmin, Zmax - Zmin) * 1.15f + wide) / speed * cycle;
		ctime += Time.deltaTime;
		if(ctime > coolingTime)
			ctime -= coolingTime;
		smr.GetComponent<Renderer>().material.SetFloat("_Dt", ctime);
	}
}
