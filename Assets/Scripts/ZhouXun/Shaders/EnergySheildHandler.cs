using UnityEngine;
using System.Collections;

public class EnergySheildHandler : MonoBehaviour
{
	public bool Demo = true;
	public Color MainColor = new Color(0, 0.075f, 0.2f, 0.2f);
	public Color HoloColor = new Color(0.15f, 0.4f, 1.0f, 0.2f);
	public float Tile = 5;
	public float BodyIntensity = 0;
	public float HoloIntensity = 0;
	public float WaveLength = 0.02f;
	public float Speed = 0.2f;
	
	float HitTime1 = 0;
	Vector3 HitPoint1 = Vector3.zero;
	float HitTime2 = 0;
	Vector3 HitPoint2 = Vector3.zero;
	float HitTime3 = 0;
	Vector3 HitPoint3 = Vector3.zero;
	int Count = 0;
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		// For demo
		if ( Demo && Input.GetMouseButtonDown(0) )
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit rch;
			if ( Physics.Raycast(ray, out rch, 1000) )
			{
				if ( rch.collider == GetComponent<Collider>() )
					Impact(rch.point);
			}
		}

		transform.rotation = Quaternion.identity;
		
		// Set material instance
		Renderer render = GetComponent<Renderer>();
		render.material.name = "Energy sheild instance";
		render.material.SetColor("_Color", MainColor);
		render.material.SetColor("_ImpactColor", HoloColor);
		render.material.SetFloat("_NoiseTile", Tile);
		render.material.SetFloat("_BodyIntensity", BodyIntensity);
		render.material.SetFloat("_HoloIntensity", HoloIntensity);
		render.material.SetFloat("_WaveLength", WaveLength);
		render.material.SetVector("_Impact1Point", new Vector4(HitPoint1.x, HitPoint1.y, HitPoint1.z, 0));
		render.material.SetFloat("_Impact1Dist", Mathf.Sqrt((Time.time - HitTime1) * Speed) - .1F);
		render.material.SetVector("_Impact2Point", new Vector4(HitPoint2.x, HitPoint2.y, HitPoint2.z, 0));
		render.material.SetFloat("_Impact2Dist", Mathf.Sqrt((Time.time - HitTime2) * Speed) - .1F);
		render.material.SetVector("_Impact3Point", new Vector4(HitPoint3.x, HitPoint3.y, HitPoint3.z, 0));
		render.material.SetFloat("_Impact3Dist", Mathf.Sqrt((Time.time - HitTime3) * Speed) - .1F);
	}

	void LateUpdate()
	{		
		transform.rotation = Quaternion.identity;
	}
	
	public void Impact( Vector3 worldpos )
	{
		if ( Count % 3 == 0 )
		{
			HitTime1 = Time.time;
			HitPoint1 = transform.InverseTransformPoint(worldpos);
		}
		else if ( Count % 3 == 1 )
		{
			HitTime2 = Time.time;
			HitPoint2 = transform.InverseTransformPoint(worldpos);					
		}
		else if ( Count % 3 == 2 )
		{
			HitTime3 = Time.time;
			HitPoint3 = transform.InverseTransformPoint(worldpos);					
		}
		Count++;
	}
}
