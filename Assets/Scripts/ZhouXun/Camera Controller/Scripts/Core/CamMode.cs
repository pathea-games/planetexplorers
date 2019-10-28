using UnityEngine;
using System.Collections;

public abstract class CamMode : CamModifier
{
	public bool m_ShowTarget = false;
	public bool m_LockCursor = false;
	public Vector2 m_TargetViewportPos = new Vector2 (0.5f, 0.5f);

	public abstract void ModeEnter ();
	public abstract void UserInput ();
}
