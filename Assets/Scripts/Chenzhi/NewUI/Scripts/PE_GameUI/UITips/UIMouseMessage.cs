using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMouseMessage : MonoBehaviour 
{

	public GameObject Target;

	public string MouseEnterFunc;
	public string MouseExitFunc;

	bool m_PrevRayCast = false;

	BoxCollider  m_BoxCollider = null;


	void Awake ()
	{
		m_BoxCollider = gameObject.GetComponent<BoxCollider>();
	}
		

	void Update ()
	{
		if (Target == null)
			return;

		if (m_BoxCollider == null)
			return;

		if (UICamera.mainCamera == null)
			return;

		Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);



		RaycastHit rch;
		bool rayCast = m_BoxCollider.Raycast(ray, out rch, 512f);

		if (m_PrevRayCast && !rayCast)
		{
			if (MouseExitFunc != "")
				Target.SendMessage(MouseExitFunc);
		}
		else if (!m_PrevRayCast && rayCast)
		{
			if (MouseEnterFunc != "")
				Target.SendMessage(MouseEnterFunc);
		}

		m_PrevRayCast = rayCast;

	}
}
