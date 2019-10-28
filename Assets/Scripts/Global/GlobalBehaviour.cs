using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlobalBehaviour : MonoBehaviour
{	
	public static string currentSceneName;
	public static string BadGfxDeviceName = null;
	public delegate bool GlobalEvent();
	static List<GlobalEvent> _golobalEventList = new List<GlobalEvent>();

	public static void RegisterEvent(GlobalEvent func)
	{
		if(!_golobalEventList.Contains(func))
		{
			_golobalEventList.Add(func);
		}
	}
	static void UnRegisterEvent(GlobalEvent func)
	{
		_golobalEventList.Remove(func);
	}
	static	void RunGolobalEvent()
	{
		foreach(var iter in _golobalEventList)
		{
			if(iter != null)
			{
				if(iter() == true)
				{
					UnRegisterEvent(iter);
					break;
				}
			}
		}
	}
	// Cons & Des
	void Awake ()
	{
		string gfxName = SystemInfo.graphicsDeviceName;
		string cpuName = SystemInfo.processorType;
		Debug.Log ("[processorType]:"+cpuName);
		Debug.Log ("[graphicsDeviceName]:"+gfxName);
		if (gfxName.Contains ("Intel")) {
			if (gfxName.Contains ("HD"))	// No include Iris
				BadGfxDeviceName = gfxName;
		} else if(gfxName.Contains("Radeon")){
			if(cpuName.Contains("APU")){
				if(cpuName.Contains("HD") && gfxName.Contains("HD")) {
					BadGfxDeviceName = gfxName;
				} else{ // Check Rx graphics
					string rx = "Rx Graphics";
					string sub = cpuName.Substring(cpuName.Length - rx.Length);
					if(gfxName.Contains(sub)){
						BadGfxDeviceName = gfxName;
					}
				}
			}
		}

		DontDestroyOnLoad(this.gameObject);

		LocalDatabase.LoadAllData();		
		SystemSettingData.Instance.LoadSystemData();

		SurfExtractorsMan.CheckGenSurfExtractor();

		PeLogicGlobal.Instance.Init();
		PeCamera.Init();
		
		StartCoroutine (Behave.Runtime.BTResolver.ApplyCacheBt());
	}

	void OnApplicationQuit()
	{
		SurfExtractorsMan.CleanUp();
		LocalDatabase.FreeAllData();
		SystemSettingData.Save();
	}

	void Update ()
	{
		PeInput.Update();
		PeEnv.Update();
		RunGolobalEvent();
	}

	void LateUpdate()
	{
		PeCamera.Update();
		currentSceneName = Application.loadedLevelName;
		SurfExtractorsMan.PostProc();
	}
}
