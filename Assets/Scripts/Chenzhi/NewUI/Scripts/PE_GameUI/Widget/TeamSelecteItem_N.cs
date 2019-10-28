using UnityEngine;
using System;
using System.Collections;

public class TeamSelecteItem_N : MonoBehaviour 
{
	public int TeamId = -1;
	public UISlicedSprite mSprite;
	public UILabel mLabel;

	void Awake()
	{
		mSprite = gameObject.GetComponentInChildren<UISlicedSprite>();
		mLabel = gameObject.GetComponentInChildren<UILabel>();
    }

	void OnClick()
	{
		if(Input.GetMouseButtonUp(0))
            RoomGui_N.Instance.ChangePlayerTeamToNet(TeamId, -1);
	}
}
