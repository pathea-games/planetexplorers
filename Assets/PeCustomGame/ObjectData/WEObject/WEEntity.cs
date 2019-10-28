using UnityEngine;
using System.Collections;

public abstract class WEEntity : WEObject
{
	[XMLIO(Attr = "player", Order = -7, DefaultValue = 0)]
	[HideInInspector] public int PlayerIndex = 0;
	
	[XMLIO(Attr = "tag", Order = 99, DefaultValue = 0f)]
	[HideInInspector] public float Tag = 0f;

	[XMLIO(Attr = "istar", Order = -7, DefaultValue = null)]
	public bool IsTarget = false;

	private bool visible = true;

	[XMLIO(Attr = "visible", Order = -7, DefaultValue = true)]
	public bool Visible
	{
		get { return visible; }
		set
		{
			visible = value;
		}
	}
}
