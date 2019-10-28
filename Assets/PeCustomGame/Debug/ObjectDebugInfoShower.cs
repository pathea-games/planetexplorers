using UnityEngine;
using System.Collections;
using Pathea;

public class ObjectDebugInfoShower : MonoBehaviour
{
	GameObject _go;

	public GUISkin gskin;
	public LayerMask layermask;

	GameObject go
	{
		get { return _go; }
		set
		{
			if (_go != value)
			{
				GameObject prev = _go;
				_go = value;
				GOChange(prev, value);
			}
		}
	}

	void GOChange (GameObject prev, GameObject now)
	{
		if (now != null)
		{
			entity = now.GetComponentInParent<PeEntity>();
		}
		else
		{
			entity = null;
		}
	}

	PeEntity entity;

	// Update is called once per frame
	void Update ()
	{
		RaycastHit rch;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rch, 256, layermask))
		{
			go = rch.collider.gameObject;
		}
		else
		{
			go = null;
		}
	}

	GUIStyle bold = null;
	GUIStyle alignright = null;

	void Start ()
	{
		bold = new GUIStyle (gskin.label);
		bold.fontStyle = FontStyle.Bold;
		bold.wordWrap = false;

		alignright = new GUIStyle (gskin.label);
		alignright.alignment = TextAnchor.LowerRight;
	}

	void OnGUI ()
	{
		GUI.skin = gskin;
		if (go != null)
		{
			float width = 220;
			float height = entity == null ? 30 : 80;
			GUI.BeginGroup(new Rect(Input.mousePosition.x + 10, Screen.height - Input.mousePosition.y, width, height));
			GUI.Box(new Rect(0,0,width,height), "");
			if (entity != null)
			{
				if (PeCreature.Instance.mainPlayer != entity)
					GUI.color = Color.yellow;
				else
					GUI.color = Color.green;
				
				GUI.Label(new Rect(5,5,width-10,20), entity.gameObject.name, bold);
				GUI.color = Color.white;

				GUI.Label(new Rect(8,30,width-16,20), "Entity ID:");
				if (PeCreature.Instance.mainPlayer != entity)
					GUI.Label(new Rect(8,55,width-16,20), "Scenario ID:");
				else
					GUI.Label(new Rect(8,55,width-16,20), "Player ID:");
				
				GUI.Label(new Rect(8,30,width-16,20), entity.Id.ToString(), alignright);
				if (PeCreature.Instance.mainPlayer != entity)
					GUI.Label(new Rect(8,55,width-16,20), entity.scenarioId.ToString(), alignright);
				else
					GUI.Label(new Rect(8,55,width-16,20), PeCustom.PeCustomScene.Self.scenario.playerId.ToString(), alignright);
			}
			else
			{
				GUI.Label(new Rect(5,5,width-10,20), go.name, bold);
			}
			GUI.EndGroup();
		}
	}
}
