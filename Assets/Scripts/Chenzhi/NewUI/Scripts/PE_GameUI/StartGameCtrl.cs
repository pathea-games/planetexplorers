using UnityEngine;
using System.Collections;
using Pathea;

public class StartGameCtrl : MonoBehaviour 
{
	[HideInInspector]
	public static bool IsStarted = false;
	string[] _preloadAssets = new string[]{
		"Prefab/GlobalAssets/_Env_",
		"Prefab/GlobalAssets/Voxel Creation System",
		"Prefab/GlobalAssets/GameClient",
		"Prefab/GlobalAssets/FMOD Audio System",
        //"Prefab/GlobalAssets/RegularMemoryCleaning",
    };
	// Use this for initialization
	void Awake()
	{
        IsStarted = false;
		PECommandLine.ParseArgs ();
	}
	void Start () 
	{
		StartCoroutine (ApplyResolution());
		ApplyOclPara ();
		if (!IsStarted) {
			UILoadScenceEffect.Instance.PalyLogoTexture (LoadGlobalAssets, LoadGameMenuSence);
		}
	}
	// Apply para 
	IEnumerator ApplyResolution()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		Screen.SetResolution(PECommandLine.W, PECommandLine.H, PECommandLine.FullScreen);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		Debug.Log("Game resolution in use["+Screen.width+"X"+Screen.height+"] fs[" + Screen.fullScreen + "]");
	}
	void ApplyOclPara()
	{
		if (!string.IsNullOrEmpty (PECommandLine.OclPara)) {
			oclManager.CurOclOpt = PECommandLine.OclPara;
		}
	}
	void ApplyInvitePara()
	{
		if (!string.IsNullOrEmpty (PECommandLine.InvitePara)) {
			PeSteamFriendMgr.Instance.ReciveInvite (0, System.Convert.ToInt64 (PECommandLine.InvitePara));
			//SteamFriendPrcMgr.StartUpServerID = System.Convert.ToInt64(paramter);
			//SteamFriendPrcMgr.InviteState = INVITESTATE.E_INVITE_MAINUI;
		}
	}
	void LoadGlobalAssets()
	{
		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
		if (_preloadAssets != null) {
			int n = _preloadAssets.Length;
			for(int i = 0; i < n; i++){
				sw.Start ();
				if(_preloadAssets[i] != null){
					Instantiate(Resources.Load(_preloadAssets[i]));
				}
				sw.Stop ();
				//Debug.LogError("Load "+i+":"+sw.ElapsedMilliseconds);
				sw.Reset();
			}
		}
		ApplyInvitePara ();
	}
	void LoadGameMenuSence()
	{
		IsStarted = true;
		PeFlowMgr.Instance.LoadScene(PeFlowMgr.EPeScene.MainMenuScene);
	}
}
