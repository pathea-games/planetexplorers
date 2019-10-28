using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Projector))]

public class ProjectorColorHandler : MonoBehaviour 
{
	public Material sourceMat;
	
	public Color  mainColor;
	public Color  origColor = Color.clear;
	
	public bool   flare;
	public float  flareDuratione;
	
	private bool  isFlaring;
	private bool  forwad;
	//private float _curFlareTime;
	
	private Material _projMat;
	private Projector _Projector;
	
	private Color curColor;
	private float velocity;
	
	// Use this for initialization
	void Start () 
	{
		_projMat 	= Material.Instantiate(sourceMat) as Material;
		_Projector	= gameObject.GetComponent<Projector>();
		_Projector.material = _projMat;
	}
	
	// Update is called once per frame
	void Update () 
	{
		_projMat.SetColor("_TintColor", mainColor);
		Vector3 pos = transform.position;
		_projMat.SetVector("_CenterAndRadius", new Vector4(pos.x, pos.y, pos.z, _Projector.orthographicSize));
		
		if (flare && !isFlaring)
		{
			curColor = origColor;
			isFlaring = true;
			forwad = true;
		}
		
		if (isFlaring)
		{
			float duration = 0.5f * flareDuratione;
			if (forwad)
			{
//				_curFlareTime += Time.deltaTime;
//				
//				if (_curFlareTime >= duration)
//				{
//					_curFlareTime = duration;
//					forwad = false;
//				}
				
				curColor.r = Mathf.SmoothDamp(curColor.r,  mainColor.r, ref velocity, duration);
				curColor.g = Mathf.SmoothDamp(curColor.g,  mainColor.g, ref velocity, duration);
				curColor.b =  Mathf.SmoothDamp(curColor.b,  mainColor.b, ref velocity, duration);
				curColor.a = mainColor.a;
				
				if (Mathf.Abs(curColor.r - mainColor.r) < 0.001f 
					&& Mathf.Abs(curColor.b - mainColor.b) < 0.001f
					&& Mathf.Abs(curColor.g - mainColor.g) < 0.001f)
				{
					forwad = false;
					curColor= mainColor;
				}
 				_projMat.SetColor("_TintColor", curColor);
				//_projMat.SetColor("_TintColor", Color.Lerp(origColor, mainColor, _curFlareTime/flareDuratione));
			}
			else
			{
//				_curFlareTime -= Time.deltaTime;
//				
//				if (_curFlareTime < 0F)
//				{
//					flare 		= false;
//					isFlaring 	= false;
//					_curFlareTime = 0F;
//				}
//				
//				_projMat.SetColor("_TintColor", Color.Lerp(origColor, mainColor, _curFlareTime/flareDuratione));
				
				//float vol = 0F;
				curColor.r = Mathf.SmoothDamp(curColor.r,  origColor.r, ref velocity, duration);
				curColor.g = Mathf.SmoothDamp(curColor.g,  origColor.g, ref velocity, duration);
				curColor.b =  Mathf.SmoothDamp(curColor.b,  origColor.b, ref velocity, duration);
				curColor.a = origColor.a;
				
				if (Mathf.Abs(curColor.r - origColor.r) < 0.001f 
					&& Mathf.Abs(curColor.b - origColor.b) < 0.001f
					&& Mathf.Abs(curColor.g - origColor.g) < 0.001f)
				{
					flare 		= false;
					isFlaring 	= false;
					gameObject.SetActive (false);
				}
				
				_projMat.SetColor("_TintColor", curColor);
			}
		}
		else
		{
			_projMat.SetColor("_TintColor", origColor);
			gameObject.SetActive (false);
		}
	}
	
	void OnDestroy ()
	{
		_Projector.material = null;
		Material.Destroy(_projMat);
	}
}
