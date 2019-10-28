#define PLANET_EXPLORERS

using UnityEngine;
using System.Collections;

// The mediator class between game and Voxel Creation System
public static class VCGameMediator
{
	public static void Update ()
	{
#if PLANET_EXPLORERS
		// Planet explorers' mediator
		
		#region VCS_2_PlanetExplorers
		GameConfig.IsInVCE = VCEditor.s_Active;
		#endregion
		
		
		#region PlanetExplorers_2_VCS
		VCEditor.s_ConnectedToGame = (Pathea.PeCreature.Instance.mainPlayer != null );
		VCEditor.s_MultiplayerMode = GameConfig.IsMultiMode;
		#endregion
#endif
	}
	
	public static bool MeshVisible (CreationMeshLoader loader)
	{
#if PLANET_EXPLORERS
//		if ( PlayerFactory.mMainPlayer != null )
//			return ( (PlayerFactory.mMainPlayer.transform.position - loader.transform.position).magnitude < 100 );
//		else
			return true;
#else
		return true;
#endif
	}
	
	public static void SendIsoDataToServer (string name, string desc, byte[] preIso, byte[] isoData,string[] tags,bool sendToServer = true,ulong fileId = 0,bool free = false)
	{
#if PLANET_EXPLORERS
		//ulong hash_code = CRC64.Compute(isodata);
		//FileTransManager.RegisterISO(hash_code, isodata);
#endif

#if SteamVersion
		SteamWorkShop.SendFile(null,name, desc, preIso, isoData,tags,sendToServer,-1,fileId, free);
#else
		ulong hash_code = CRC64.Compute(isoData);
		//FileTransManager.RegisterISO(hash_code, isoData);
#endif
	}

	public static bool UseLandHeightMap ()
	{
#if PLANET_EXPLORERS
		return (Pathea.PeGameMgr.IsSingleStory);
#else
		return false;
#endif
	}

	public static void CloseGameMainCamera ()
	{
#if PLANET_EXPLORERS
		if ( Camera.main != null )
		{
			VCEditor.s_OutsideCameraCullingMask = Camera.main.cullingMask;
			Camera.main.cullingMask = 0;
			if ( PECameraMan.Instance != null )
				PECameraMan.Instance.m_Controller.SaveParams();
		}
#else
#endif
	}

	public static void RevertGameMainCamera ()
	{
#if PLANET_EXPLORERS
		if ( Camera.main != null )
		{
			Camera.main.cullingMask = VCEditor.s_OutsideCameraCullingMask;
			if ( PECameraMan.Instance != null )
				PECameraMan.Instance.m_Controller.SaveParams();
		}
#else
#endif
	}

	public static float SEVol
	{
		get 
		{
#if PLANET_EXPLORERS
			return SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.EffectVolume;
#else
			return 1f;
#endif
		}
	}
}
