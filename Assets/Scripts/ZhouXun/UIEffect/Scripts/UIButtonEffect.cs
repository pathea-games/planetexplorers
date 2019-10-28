using UnityEngine;
using System.Collections;

public class UIButtonEffect : MonoBehaviour
{
	[SerializeField] UISpecularHandler ButtonTex;
	[SerializeField] Shader BraceShader;
	[SerializeField] Texture2D BraceTex;
	UITexture BraceLT;
	UITexture BraceLB;
	UITexture BraceRT;
	UITexture BraceRB;

	float t = 0;
	[SerializeField] float During = 0.40f;
	[SerializeField] float Direction = 0;
	[SerializeField] AnimationCurve OffsetChange;
	[SerializeField] Gradient ColorChange;

	bool isMouseDown;

	public void MouseEnter ()
	{
		Direction = 1;
	}

	public void MouseLeave ()
	{
		Direction = -2;
	}

	public void MouseDown ()
	{
		isMouseDown = true;
	}
	
	public void MouseUp ()
	{
		isMouseDown = false;
	}

	void Awake ()
	{
		GameObject go = new GameObject ("brace");
		go.transform.parent = transform;
		go.layer = this.gameObject.layer;
		BraceLT = go.AddComponent<UITexture>();
		BraceLT.shader = BraceShader;
		BraceLT.mainTexture = BraceTex;
		go = new GameObject ("brace");
		go.transform.parent = transform;
		go.layer = this.gameObject.layer;
		BraceLB = go.AddComponent<UITexture>();
		BraceLB.shader = BraceShader;
		BraceLB.mainTexture = BraceTex;
		go = new GameObject ("brace");
		go.transform.parent = transform;
		go.layer = this.gameObject.layer;
		BraceRT = go.AddComponent<UITexture>();
		BraceRT.shader = BraceShader;
		BraceRT.mainTexture = BraceTex;
		go = new GameObject ("brace");
		go.transform.parent = transform;
		go.layer = this.gameObject.layer;
		BraceRB = go.AddComponent<UITexture>();
		BraceRB.shader = BraceShader;
		BraceRB.mainTexture = BraceTex;

		BraceLT.transform.localScale = new Vector3 (8, 24, 1);
		BraceLB.transform.localScale = new Vector3 (8, -24, 1);
		BraceRT.transform.localScale = new Vector3 (-8, 24, 1);
		BraceRB.transform.localScale = new Vector3 (-8, -24, 1);
	}

	void Reset ()
	{
		t = 0;
		Direction = 0;

		if (BraceLT != null)
			BraceLT.gameObject.SetActive(false);
		
		if (BraceLB != null)
			BraceLB.gameObject.SetActive(false);
		
		if (BraceRT != null)
			BraceRT.gameObject.SetActive(false);
		
		if (BraceRB != null)
			BraceRB.gameObject.SetActive(false);
	}

	void OnEnable ()
	{
		Reset();
	}
	
	void OnDisable ()
	{
		Reset();
	}

	void OnDestroy ()
	{
		if (BraceLT != null)
			GameObject.Destroy(BraceLT.gameObject);

		if (BraceLB != null)
			GameObject.Destroy(BraceLB.gameObject);

		if (BraceRT != null)
			GameObject.Destroy(BraceRT.gameObject);

		if (BraceRB != null)
			GameObject.Destroy(BraceRB.gameObject);
	}
	
	// Update is called once per frame
	void Update ()
	{
		t += Time.deltaTime / During * Direction;
		t = Mathf.Clamp01(t);
		
		if (t <= 0)
		{
			BraceLT.gameObject.SetActive(false);
			BraceLB.gameObject.SetActive(false);
			BraceRT.gameObject.SetActive(false);
			BraceRB.gameObject.SetActive(false);
		}
		else
		{
			BraceLT.gameObject.SetActive(true);
			BraceLB.gameObject.SetActive(true);
			BraceRT.gameObject.SetActive(true);
			BraceRB.gameObject.SetActive(true);
		}

		float intens = isMouseDown ? 0.7f : 1f;
		float rot = isMouseDown ? 180f : 0f;
		float pofs = isMouseDown ? -1f : 0f;
		Color tint = Color.black;
		if (Direction > 0.001f)
			tint = ColorChange.Evaluate(t);
		else
			tint = ColorChange.Evaluate(1) * t;
		tint *= intens;
		BraceLT.color = tint;
		BraceLB.color = tint;
		BraceRT.color = tint;
		BraceRB.color = tint;

		ButtonTex.transform.eulerAngles = new Vector3(0,0,rot);
		Vector3 size = ButtonTex.transform.localScale;
		float xofs = size.x/2 - 2 + Mathf.Max(0, OffsetChange.Evaluate(t)) + pofs;
		float yofs = size.y/2 - 7 + Mathf.Max(0, OffsetChange.Evaluate(t)) + pofs;
		float zofs = ButtonTex.transform.localPosition.z;

		BraceLT.transform.localPosition = new Vector3 (-xofs, yofs, zofs);
		BraceLB.transform.localPosition = new Vector3 (-xofs, -yofs, zofs);
		BraceRT.transform.localPosition = new Vector3 (xofs, yofs, zofs);
		BraceRB.transform.localPosition = new Vector3 (xofs, -yofs, zofs);
	}
}
