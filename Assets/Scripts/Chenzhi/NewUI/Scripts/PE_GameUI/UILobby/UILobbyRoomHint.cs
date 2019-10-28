using UnityEngine;
using System.Collections;

public class UILobbyRoomHint : MonoBehaviour 
{

	UILobbyMainWndCtrl mLobbyWndCtrl;
	//ServerRegistered mServerData = null;
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}


	void OnTooltip (bool show)
	{
		if (show)
		{
			//LobbyToolTipMgr.ShowText("test");
		}
		else
		{
			//LobbyToolTipMgr.ShowText(null);
		}
	}

}
