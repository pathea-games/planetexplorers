using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class TrailLattice
{
	public Vector3 startPos;
    public Vector3 endPos;
    public float time;
	public TrailLattice(Vector3 s, Vector3 e, float t){
		startPos = s;
		endPos = e;
		time = t;
	}
}
public class testWeaponTrail : MonoBehaviour {
	public int splitCount = 10;
	public float lenth = 0.5f;
	public float lifeTime = 0.5f;
	public float bufferTime = 0.5f;
	public bool start = true; 

	//for change trail shape
	Mesh mesh;
	Vector3[] vertices;
	Color[] colors;
	Vector2[] uv;
	int[] triangles;
	//MeshRenderer meshRender;
	//Material mat;
	Color startColor = Color.white;
	Color endColor = new Color(1, 1, 1, 0);

	//for calculate trail shape
	float time = 0f;
	float needTime = 2f;
	float timeTransitionSpeed = 1f;
	Vector3 lastPos = Vector3.zero;
	Vector3 lastFwd = Vector3.zero;
	Vector3 currentPos = Vector3.zero;
	Vector3 currentFwd = Vector3.zero;
	float tempRadius;
	float percent;
	float u;
	Vector3 tempFwd;
	Vector3 tempStart;
	TrailLattice tempLat;
	List<TrailLattice> lattice = new List<TrailLattice>();


	void Awake()
	{
		mesh = this.GetComponent<MeshFilter>().mesh;
		//meshRender = this.GetComponent<MeshRenderer>();
		//mat = meshRender.material;
	}

	void Start()
	{

	}

	void Update()
	{
		if(start)
			StartTrail(lifeTime, bufferTime);
		currentPos = transform.position;
		currentFwd = transform.forward;
	}

	void LateUpdate()
	{
		float mAnimTime = Mathf.Clamp(Time.deltaTime * 1, 0, 0.066f);
		Itterate();
		if (time > 0)
			UpdateTrail (Time.time, mAnimTime);
	}
	
	void StartTrail(float timeToTweenTo, float fdTime)
	{		
		needTime = timeToTweenTo;
		if (time != needTime){
			timeTransitionSpeed = Mathf.Abs(needTime - time) / fdTime;
		}
		if (time <= 0){
			time = 0.01f;
		}
		currentPos = transform.position;
		currentFwd = transform.forward;
		lastPos = transform.position;
		lastFwd = transform.forward;
		//gameObject.SetActive(true);
		start = false;
	}

	void ClearTrail()
	{
		needTime = 0;
		time = 0;
		if (mesh != null) {
			mesh.Clear();
			lattice.Clear();
		}
		start = false;
	}

	void UpdateTrail(float time_Time, float deltaTime)
	{
		mesh.Clear();
		while (lattice.Count > 0 && time_Time > lattice[lattice.Count - 1].time + time) {
			lattice.RemoveAt(lattice.Count - 1);
		}
		vertices = new Vector3[lattice.Count * 2];
		uv = new Vector2[lattice.Count * 2];
		colors = new Color[lattice.Count * 2];
		triangles = new int[lattice.Count * 6 - 6];
		for(int i = 0; i < lattice.Count; i++)
		{
			tempLat = lattice[i];
			vertices[i * 2] = transform.InverseTransformPoint(tempLat.startPos);
			vertices[i * 2 + 1] = transform.InverseTransformPoint(tempLat.endPos);
			u = Mathf.Clamp01((time_Time - tempLat.time) / time);
			uv[i * 2] = new Vector2(u, 0);
			uv[i * 2 + 1] = new Vector2(u, 1);
			colors[i * 2 + 1] = colors[i * 2] = Color.Lerp(startColor, endColor, u);

		}
		for(int i = 0; i < lattice.Count - 1; i++)
		{
			triangles[i * 6] = i * 2;
			triangles[i * 6 + 1] = i * 2 + 1;
			triangles[i * 6 + 2] = i * 2 + 2;
			triangles[i * 6 + 3] = i * 2 + 2;
			triangles[i * 6 + 4] = i * 2 + 1;
			triangles[i * 6 + 5] = i * 2 + 3;
		}
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.colors = colors;
		mesh.triangles = triangles;
		if (time > needTime)
		{
			time -= deltaTime * timeTransitionSpeed;
			if(time <= needTime) 
				time = needTime;
		} else if (time < needTime)
		{
			time += deltaTime * timeTransitionSpeed;
			if(time >= needTime)
				time = needTime;
		}
	}
	
	public void Itterate()
	{
		if(time <= 0)
			ClearTrail();
		lattice.Insert(0, new TrailLattice(lastPos, lastFwd * lenth + lastPos, Time.time - Time.deltaTime));
		//tempRadius = Vector3.Distance(lastPos, transform.position) * 0.5f / Mathf.Sin(Vector3.Angle(lastFwd, transform.forward) / 360f * Mathf.PI);
		for(int i = 1; i < splitCount ; i++)
		{
			percent = i / (splitCount - 1f);
			//tempFwd = Vector3.Slerp(lastFwd, transform.forward, percent);
			//tempStart = lastPos + (tempFwd - lastFwd) * tempRadius;
			//tempFwd = Vector3.Lerp(lastFwd, transform.forward, (2f - percent) * percent);
			//tempStart = Vector3.Lerp(lastPos, transform.position, (2f - percent) * percent);
			tempFwd = Vector3.Lerp(Vector3.Lerp(lastFwd, currentFwd, percent), transform.forward, percent);
			tempStart = Vector3.Lerp(Vector3.Lerp(lastPos, currentPos, percent), transform.position, percent);
			lattice.Insert(0, new TrailLattice(tempStart, tempStart + tempFwd * lenth, Time.time + (percent - 1) * Time.deltaTime));
		}
		lastPos = transform.position;
		lastFwd = transform.forward;
	}
}